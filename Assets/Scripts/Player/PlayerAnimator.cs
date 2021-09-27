using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{

    public Animator animator;
    public PlayerController controller;

    public Texture2D[] JumpFrames;
    public GameObject spriteObject;

    public BoxCollider2D boxCollider;
    private Vector2 colliderSize;
    // Start is called before the first frame update
    void Start()
    {
        colliderSize = boxCollider.size;
    }

    

    // Update is called once per frame
    void Update()
    {
        
        animator.SetBool("GoingUp", false);
        animator.SetBool("GoingDown", false);
        animator.SetBool("Prone", false);
        animator.SetBool("ProneIdle", false);
        animator.SetBool("Slide", false);
        animator.SetBool("Running", false);
        animator.SetBool("Walking", false);

        boxCollider.size = new Vector2(controller.isSliding ? colliderSize.x * 2 : colliderSize.x, controller.isCrouching ? controller.CrouchPercent * colliderSize.y: colliderSize.y);

        if (controller.isSliding || controller.isCrouching) {
            spriteObject.transform.localPosition = new Vector3(0, controller.CrouchPercent * colliderSize.y * 0.5f, 0f);
        } else {
            spriteObject.transform.localPosition = Vector3.zero;
        }

        if (controller.isSliding) {
            animator.SetBool("Slide", true);
        }else if (controller.isCrouching) {
            animator.SetBool("ProneIdle", true);
            if (controller.isWalking) {
                animator.SetBool("Prone", true);
            }
        } else if (!controller.PlayerCollider.isGrounded) {
            if (controller.VerticalVelocity > 0) {
                animator.SetBool("GoingUp", true);
            } else {
                animator.SetBool("GoingDown", true);
            }
        } else {

            animator.SetBool(controller.SlowMode ? "Walking" : "Running", controller.isWalking);

        }
        animator.Update(0);
        animator.Update(0);
        animator.Update(0);
    }

}
