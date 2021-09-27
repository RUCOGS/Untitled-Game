using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeath : MonoBehaviour
{

    public float DeathBarrierFall;

    public Vector3 spawnInPosition;

    public void SetDeathPoint(Vector3 pos) {
        spawnInPosition = pos;
    }

    public void SetDeathPoint(Transform tran){
        spawnInPosition = tran.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        spawnInPosition = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        

        if(this.transform.position.y < DeathBarrierFall) {
            Die();
        }
    }

    public void Die() {
        this.transform.position = spawnInPosition;
    }
}
