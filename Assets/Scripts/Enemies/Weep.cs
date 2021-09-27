using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weep : MonoBehaviour {

    public HealthManager healthManager;

    public AudioRandomClip randomClip;

    private void Start() {
        healthManager.OnDamaged.AddListener(OnHit);
    }


    public void OnHit(int i) {
        randomClip.Play();
    }

    void Update()
    {
        if (healthManager.Health <= 0) {
            Destroy(gameObject);
        }
    }
}
