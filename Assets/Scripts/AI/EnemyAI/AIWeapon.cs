using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIWeapon : PoolBehaviour
{

    float TimeSinceLastShot;

    public float CooldownSeconds;
    public int Damage;

    public void Shoot(Vector3 Direction, float Velocity) {

        if(TimeSinceLastShot + CooldownSeconds < Time.time) {

            TimeSinceLastShot = Time.time;

            var projectile = (FireballProjectile)this.RetrieveObject();
            projectile.StartMovement(this.transform.position, Direction, Velocity, Damage);

        }

    }
}
