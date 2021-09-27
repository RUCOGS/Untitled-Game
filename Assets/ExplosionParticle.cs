using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionParticle : MonoBehaviour {
    void Start() {
        gameObject.GetComponent<Rigidbody2D>().AddForceAtPosition(
            new Vector2(Random.Range(5f, 10f), Random.Range(1f, 3f)), 
            new Vector2(Random.Range(.1f, .1f), Random.Range(.1f, .1f)),
            ForceMode2D.Impulse
        );
    }
    void Update()
    {
        
    }
}
