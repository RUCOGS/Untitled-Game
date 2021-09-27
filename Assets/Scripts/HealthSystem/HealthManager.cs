using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class HealthBase : MonoBehaviour {


    public abstract void Damage(int amount);
    public abstract void Heal(int amount);

    public abstract int dHealth {
        get;
        set;
    }
    public abstract int dMaxHealth {
        get;
        set;
    }

}

[System.Serializable]
public class HealthIntEvent : UnityEvent<int>{}

public class HealthManager : HealthBase
{
    public HealthIntEvent OnChange = new HealthIntEvent();

    public HealthIntEvent OnHeal = new HealthIntEvent();

    public HealthIntEvent OnDamaged = new HealthIntEvent();

    public UnityEvent OnDeath;


    [Header("Health Settings")]
    public bool AllowNegativeHealth;

    [Space(4)]
    [Header("Health Values")]
    public int Health;
    public int MaxHealth;

    public override int dHealth { 
        get {
            return Health;
        }
        set {
            Health = value;
        } 
    }

    public override int dMaxHealth {
        get {
            return MaxHealth;
        }
        set {
            MaxHealth = value;
        }
    }


    public override void Damage(int amount) {

        if(amount <= 0) {
            ConsoleLogger.debug("HealthManager", "Damage amount should neither be 0 or negative!");
        }

        Health -= amount;

        if(Health <= 0) {

            if (!AllowNegativeHealth) {
                Health = 0;
            }

            if (OnDeath != null) OnDeath.Invoke();

        }

        
        if(OnDamaged != null) OnDamaged.Invoke(amount);
        if (OnChange != null) OnChange.Invoke(-amount);
    }

    public override void Heal(int amount) {

        if (amount <= 0) {
            ConsoleLogger.debug("HealthManager", "Heal amount should neither be 0 or negative!");
        }


        Health += amount;

        if(Health > MaxHealth) {
            Health = MaxHealth;
        }

        if (OnHeal != null) OnHeal.Invoke(amount);
        if (OnChange != null) OnChange.Invoke(amount);
    }


    /// <summary>
    /// Links all health references that is child to this transform
    /// </summary>
    [ContextMenu("Construct References")]
    public void LinkAllReferences(){

        Queue<Transform> CrawlQueue = new Queue<Transform>();
        CrawlQueue.Enqueue(this.transform);

        while (CrawlQueue.Count > 0) {

            Transform cur = CrawlQueue.Dequeue();

            if(cur != null) {

                HealthReference reference = cur.GetComponent<HealthReference>();

                if(reference != null) {
                    reference.MainHealth = this;
                }

                foreach(Transform child in cur) {
                    CrawlQueue.Enqueue(child);
                }

            }

        }


    }

}
