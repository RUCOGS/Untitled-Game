using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Main reference to player
/// </summary>
[RequireComponent(typeof(PlayerController), typeof(RaycastCollider2D), typeof(HealthManager))]
public class Player : MonoBehaviour
{

    public static Player instance;

    //Audio should be swaped over to new class
    public AudioRandomClip JumpSource;
    public AudioRandomClip FootstepSource;

    [HideInInspector]
    public PlayerController controller;
    [HideInInspector]
    public RaycastCollider2D raycastCollider;
    [HideInInspector]
    public HealthManager healthManager;
    [HideInInspector]
    public PlayerDeath death;
    [HideInInspector]
    public CameraShaker camShaker;


    public PlayerInventory inventory;
    public CinemachineVirtualCamera PlayerCamera;

    public void Teleport(Vector3 pos) {

        Vector3 delta = this.transform.position - pos;
        this.transform.position = pos;
        PlayerCamera.PreviousStateIsValid = false;
        PlayerCamera.OnTargetObjectWarped(this.transform, delta);
    }

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Debug.LogError("MULTIPLE PLAYERS ON SCENE. THIS BREAKS EVERYTHING!");
        }
    }

    private void OnDestroy() {
        instance = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        controller = this.GetComponent<PlayerController>();
        raycastCollider = this.GetComponent<RaycastCollider2D>();

        if (inventory == null) {
            Debug.LogError("Reference to inventory not set! THIS IS REQUIRED");
        }

        healthManager = this.GetComponent<HealthManager>();
        death = this.GetComponent<PlayerDeath>();

    }

    public float FootstepTimer;
    private float curFootstepTime;
    // Update is called once per frame
    void Update()
    {


        
        if (controller.isWalking && raycastCollider.isGrounded) {
            curFootstepTime += Time.deltaTime;
            if(curFootstepTime >= FootstepTimer * (controller.SlowMode ? 2 : 1)) {
                curFootstepTime = 0;
                FootstepSource.Play();
            }
        } else {
            curFootstepTime = FootstepTimer;
        }

        if (controller.justJumped) {
            JumpSource.Play();
        }

        if (healthManager.Health <= 0) {
            death.Die();
            healthManager.Heal(int.MaxValue);
        }
    }
}
