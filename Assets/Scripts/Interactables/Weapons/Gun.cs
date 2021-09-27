using UnityEditor;
using UnityEngine;

public abstract class Gun : Weapon {

    private float timeSinceLastShot;
    public float shotCooldown;
    public bool canShoot = true;
    public float maxShotRange = 25;

    public float holdDist;
    
    private SpriteRenderer[] spriteRenderers;

    protected override void Start() {
        base.Start();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    protected override void Update() {
        base.Update();
        timeSinceLastShot += Time.deltaTime;
        if (timeSinceLastShot > shotCooldown) {
            canShoot = true;
        }
    }

    protected override void Attack() {
        if (canShoot) {
            timeSinceLastShot = 0f;
        }
    }
    
    public void flipY(bool flipY) {
        foreach (var spriteRenderer in spriteRenderers) {
            var srGo = spriteRenderer.gameObject;
            spriteRenderer.flipY = flipY;
            if (srGo.name == "Muzzle") {
                var pos = srGo.transform.localPosition;
                srGo.transform.localPosition = new Vector3(pos.x, flipY ? -0.04f : 0.04f, pos.z);
            }
        }
    }
}
