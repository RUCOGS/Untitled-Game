using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthReference : HealthBase {

    public HealthManager MainHealth;

    public override void Damage(int amount) {
        MainHealth.Damage(amount);
    }

    public override void Heal(int amount) {
        MainHealth.Heal(amount);
    }

    public override int dHealth {
        get {
            return MainHealth.Health;
        }
        set {
            MainHealth.Health = value;
        }
    }

    public override int dMaxHealth {
        get {
            return MainHealth.MaxHealth;
        }
        set {
            MainHealth.MaxHealth = value;
        }
    }
}
