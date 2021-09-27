using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthDebugger : MonoBehaviour
{

    public HealthBase health;

    public int DamageAmount;
    public int HealAmount;

    public float TickInterval;


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Ticker());
    }

    public IEnumerator Ticker() {

        while (true) {

            if(DamageAmount > 0)health.Damage(DamageAmount);
            if(HealAmount > 0)health.Heal(HealAmount);

            yield return new WaitForSeconds(TickInterval);
        }

    }

}
