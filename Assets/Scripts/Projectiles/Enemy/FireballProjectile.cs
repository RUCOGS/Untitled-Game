using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballProjectile : MonoBehaviour, IPoolable {


    //Interface implementations
    public void Pool() {
        this.gameObject.SetActive(false);

        
    }

    public void Unpool() {
        Velocity = 0.0f;
        AimDirection = Vector3.zero;
        currentTimeout = 0;
        this.gameObject.SetActive(true);
    }

    public void Clean() {
        if (this != null) {
            Destroy(this.gameObject);
        }
    }

    public PoolBehaviour parentPool;

    public PoolBehaviour poolParent { 
        get {
            return parentPool;
        }
        set {
            parentPool = value;
        } 
    }

    //Projectile code

    [HideInInspector]
    public Vector3 AimDirection;
    [HideInInspector]
    public float Velocity;

    public float CollisionRadius;
    public LayerMask CollisionMask;
    public float Timeout;
    private float currentTimeout;

    private int DamageAmount;

    public void StartMovement(Vector3 pos, Vector3 AimDir, float velocity, int DamageAmount) {
        Velocity = velocity;
        AimDirection = AimDir;
        this.transform.position = pos;

        var angle = Mathf.Atan2(AimDir.y, AimDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        this.DamageAmount = DamageAmount;
    }

    public void Update() {
        if (gameObject.activeInHierarchy) {
            currentTimeout += Time.deltaTime;

            if(currentTimeout > Timeout) {
                this.poolParent.PoolObject(this);
            }

            float speed = Velocity * Time.deltaTime;

            RaycastHit2D hit2D = Physics2D.CircleCast(this.transform.position, CollisionRadius, AimDirection, speed, CollisionMask);

            if(hit2D) {
                OnHit(hit2D);
            } else {
                this.transform.position = this.transform.position + (AimDirection * Velocity * Time.deltaTime);
            }

        }

    }

    public void OnHit(RaycastHit2D hit) {

        HealthBase health = hit.collider.gameObject.GetComponent<HealthBase>();

        if(health != null) {
            health.Damage(DamageAmount);
        }

        if(this.poolParent == null) {
            Destroy(this.gameObject);
        } else {
            this.poolParent.PoolObject(this);
        }

    }
}
