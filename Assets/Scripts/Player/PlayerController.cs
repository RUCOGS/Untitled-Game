using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This may want to be moved to a scriptable object asset
[Serializable]
public struct PlayerControllerSettings {
    [Header("General")]
    [Space(4)]
    public float Gravity;
    public float Mass;
    public float GravityTerminalVelocity;
    public float JumpVelocity;
    public float JumpHoldTime;
    [Range(0, 1)]
    public float HeadBounce;

    public float HorizontalSpeed;

    public float CoyoteTime;


    [Range(0, 1)]
    public float CrouchPercent;

    [Header("Crouching")]
    [Space(4)]
    public float CrouchGravity;
    public float CrouchJumpVelocity;
    public float CrouchMovementSpeed;

    [Header("Sliding")]
    [Space(4)]
    public float slidingVelocity;
    public float slidingVelocityDrag;
    public float slidingAirDrag;
    public float slidingMinVelocity;
}


public class PlayerController : MonoBehaviour
{


    [Header("Activations")]
    [Space(4)]
    public bool EnableJump;
    public bool EnableSlide;
    public bool EnableCrouch;
    public bool EnableMove;
    public bool SlowMode;



    [Space(4)]
    [Header("References")]
    public RaycastCollider2D PlayerCollider;
    public SpriteRenderer sRenderer;
    private BoxCollider2D ColliderBox;

    [Space(4)]
    [Header("General settings")]
    public PlayerControllerSettings settings;
    public PlayerControllerSettings slowSettings;

    private PlayerControllerSettings curSettings;
    
    
    //Jumping/Crouch vars
    private float initialHeight;
    private bool isJumping = false;
    private bool CanJumpCoyote = false;
    private bool tryingToUncrouch;
    
    //Status
    public bool isWalking { get; private set; }
    public bool isCrouching { get; private set; }
    public bool isSliding { get; private set; }
    public float VerticalVelocity { get; private set; }
    public float CrouchPercent { get { return curSettings.CrouchPercent; } }
    //Sliding Vars
    private float currentSlidingVelocity = 0;
    private Vector3 currentSlidingDirection = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        ColliderBox = GetComponent<BoxCollider2D>();
        initialHeight = ColliderBox.size.y;
        curSettings = settings;
    }

    private Coroutine JumpCoroutine;
    private Coroutine CoyoteCoroutine;
    private bool WasGrounded = false;
    // Update is called once per frame
    void Update()
    {

        if (SlowMode) {
            curSettings = slowSettings;
        } else {
            curSettings = settings;
        }

        //Crouching

        if (CanActivateCrouch(true)) {

            Crouch();

            //Slide activation point
            if (PlayerCollider.isGrounded && (Input.GetKey(KeyCode.A) ^ Input.GetKey(KeyCode.D) && CanActivateSlide())) {
                isSliding = true;
                currentSlidingVelocity = curSettings.slidingVelocity;

                currentSlidingDirection = Input.GetKey(KeyCode.A) ? Vector3.left : Vector3.right;
            }

        } else {
            if (!Input.GetKey(KeyCode.LeftControl)) {
                UpdateCrouchSlide();
            }

        }


        PlayerCollider.UpdateCollisions();

        //Sliding

        if (!PlayerCollider.isGrounded || isSliding) {
            currentSlidingVelocity -= currentSlidingVelocity * (PlayerCollider.isGrounded ? curSettings.slidingVelocityDrag : curSettings.slidingAirDrag) * Time.deltaTime;

            //If we change directions break the slide momentum
            
            Vector3 changeDirection = currentSlidingDirection;

            bool checkHit = false;

            if (CanMove(KeyCode.A)) {
                changeDirection = Vector3.left;

                if (PlayerCollider.isCollidingLeft) {
                    checkHit = true;
                }
            } else if (CanMove(KeyCode.D)) {
                changeDirection = Vector3.right;

                if (PlayerCollider.isCollidingRight) {
                    checkHit = true;
                }
            }

            if (changeDirection != currentSlidingDirection || checkHit) {
                isSliding = false;
                currentSlidingVelocity = 0;
                currentSlidingDirection = Vector3.zero;
            }
            if (currentSlidingVelocity - curSettings.slidingMinVelocity <= 0) {
                isSliding = false;
                currentSlidingDirection = Vector3.zero;
            } else {
                this.transform.position += currentSlidingDirection * currentSlidingVelocity * Time.deltaTime;
            }
        } else {
            currentSlidingVelocity = 0;
            currentSlidingDirection = Vector3.zero;
        }



        if ((isCrouching && !isSliding )|| VerticalVelocity > 0) {
            PlayerCollider.PassthroughOneways = true;
        } else {
            PlayerCollider.PassthroughOneways = false;
        }

        //Walking
        isWalking = false;

        Vector3 Displacement = Vector3.zero;

        if (!PlayerCollider.isCollidingLeft && CanMove(KeyCode.A) && !(currentSlidingDirection == Vector3.left && isSliding)) {

            if (!PlayerCollider.isGrounded && currentSlidingVelocity > 0 && currentSlidingDirection == Vector3.left) {

            } else {
                Displacement += Vector3.left;
                sRenderer.flipX = true;
                isWalking = true;
            }

        }

        if (!PlayerCollider.isCollidingRight && CanMove(KeyCode.D) && !(currentSlidingDirection == Vector3.right && isSliding)) {

            if (!PlayerCollider.isGrounded && currentSlidingVelocity > 0 && currentSlidingDirection == Vector3.right) {

            } else {
                Displacement += Vector3.right;
                sRenderer.flipX = false;
                isWalking = true;
            }

            
        }

        

        Displacement *= (isCrouching ? curSettings.CrouchMovementSpeed : curSettings.HorizontalSpeed) * Time.deltaTime;
        this.transform.position += Displacement;



        //Jump physics
        if (WasGrounded && !PlayerCollider.isGrounded) {

            CoyoteCoroutine = StartCoroutine(CoyoteJumpTimer());
            WasGrounded = false;

        } else if (!WasGrounded && PlayerCollider.isGrounded) {

            if (CoyoteCoroutine != null) {
                StopCoroutine(CoyoteCoroutine);
                CoyoteCoroutine = null;
            }
        }

        WasGrounded = PlayerCollider.isGrounded;

        //Jump checking
        if (CanActivateJump(true) && (PlayerCollider.isGrounded || CanJumpCoyote) && !(tryingToUncrouch && !CanActivateCrouch())) {

            Jump();

        }

        if(PlayerCollider.isCollidingTop && (isJumping || VerticalVelocity > 0)) {
            if (JumpCoroutine != null) StopCoroutine(JumpCoroutine);
            VerticalVelocity = VerticalVelocity * -curSettings.HeadBounce;
            isJumping = false;
            JumpCoroutine = null;
        }

        if(!CanActivateJump() && JumpCoroutine != null) {
            StopCoroutine(JumpCoroutine);
            isJumping = false;
            JumpCoroutine = null;
        }


        if (isJumping) {
            VerticalVelocity = (isCrouching ? curSettings.CrouchJumpVelocity : curSettings.JumpVelocity);
        } else {
            if (!PlayerCollider.isCollidingBottom) {
                VerticalVelocity -= (isCrouching ? curSettings.CrouchGravity : curSettings.Gravity) * curSettings.Mass * Time.deltaTime;

                VerticalVelocity = Mathf.Max(curSettings.GravityTerminalVelocity, VerticalVelocity);
            } else {
                VerticalVelocity = 0;
            }
        }


        this.transform.position += Vector3.up * VerticalVelocity * Time.deltaTime;

        

    }


    public void Jump() {
        if (JumpCoroutine != null) StopCoroutine(JumpCoroutine);

        JumpCoroutine = StartCoroutine(JumpHoldTimer());
    }

    public void UpdateCrouchSlide() {

        


        if (!tryingToUncrouch) {
            return;
        }


        Uncrouch();
        PlayerCollider.UpdateCollisions();

        if (PlayerCollider.isCollidingBottom && PlayerCollider.isCollidingTop) {
            Crouch();
            PlayerCollider.UpdateCollisions();
        } else {
            tryingToUncrouch = false;
            isSliding = false;
        }
    }

    private void Crouch() {
        isCrouching = true;
        tryingToUncrouch = true;
        ColliderBox.size = new Vector2(ColliderBox.size.x, initialHeight * curSettings.CrouchPercent);
       
        //This raycast prevents the player from crouching through terrain thus getting stuck in terrain
        RaycastHit2D hit;
        hit = Physics2D.Raycast(this.transform.position, Vector2.down, initialHeight * curSettings.CrouchPercent * 0.5f, PlayerCollider.collisionMask);
        if (hit) {
            this.transform.position -= new Vector3(0, Math.Abs(this.transform.position.y - hit.point.y) * 0.5f);
        } else {
            this.transform.position -= new Vector3(0, (initialHeight - (initialHeight * curSettings.CrouchPercent)) * 0.5f);
        }

        this.PlayerCollider.UpdateCollisions();
        this.PlayerCollider.UpdateUnstucks();
    }

    private void Uncrouch() {
        isCrouching = false;
        ColliderBox.size = new Vector2(ColliderBox.size.x, initialHeight);
        this.transform.position += new Vector3(0, (initialHeight - (initialHeight * curSettings.CrouchPercent)) * 0.5f);
    }


    public bool CanActivateCrouch(bool onDown = false) {
        return (onDown ? Input.GetKeyDown(KeyCode.LeftControl) : Input.GetKey(KeyCode.LeftControl)) && EnableCrouch;
    }

    public bool CanActivateSlide() {
        return Input.GetKeyDown(KeyCode.LeftControl) && EnableSlide;
    }

    public bool CanActivateJump(bool onDown = false) {
        return (onDown ? Input.GetKeyDown(KeyCode.Space) : Input.GetKey(KeyCode.Space)) && EnableJump;
    }

    public bool CanMove(KeyCode code) {
        return Input.GetKey(code) && EnableMove;
    }


    public void SetEnableCrouch(bool value) {
        EnableCrouch = value;
    }
    public void SetEnableJump(bool value) {
        EnableJump = value;
    }
    public void SetEnableSlide(bool value) {
        EnableSlide = value;
    }
    public void SetSlowMode(bool value) {
        SlowMode = value;
    }

    public bool justJumped = false;
    public IEnumerator JumpHoldTimer(){
        isJumping = true;
        justJumped = true;
        yield return new WaitForEndOfFrame();
        justJumped = false;
        yield return new WaitForSeconds(curSettings.JumpHoldTime);
        isJumping = false;

        JumpCoroutine = null;
    }

    public IEnumerator CoyoteJumpTimer() {
        CanJumpCoyote = true;
        yield return new WaitForSeconds(curSettings.CoyoteTime);
        CanJumpCoyote = false;

        CoyoteCoroutine = null;
    }


}
