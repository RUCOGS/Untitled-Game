using UnityEngine;
using UnityEngine.Tilemaps;

public class TntExplosion: WorldEffectedItem {

    public int numParticles;
    public float particleTime;
    
    public GameObject explosionParticle;
    
    private TilemapCollider2D tCol;
    private TilemapRenderer tRen;
    private AudioSource aud;

    private void Start() {
        tCol = GetComponentInParent<TilemapCollider2D>();
        tRen = GetComponentInParent<TilemapRenderer>();
        aud = GetComponent<AudioSource>();
    }

    public override void Effect() {
        aud.PlayDelayed(.2f);
        tCol.enabled = false;
        tRen.enabled = false;
        for (int i = 0; i < numParticles; i++) {
            Destroy(Instantiate(explosionParticle, transform), particleTime);
        }
    }
    
}
