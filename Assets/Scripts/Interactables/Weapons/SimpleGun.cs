using UnityEngine;

public class SimpleGun : Gun {
    public float recoil;
    public int damage;

    private PlayShootingEffects muzzle;
    private Transform muzzleTransform;

    private float gunSpriteScl;
    private float gunSpriteWidth;

    public AudioRandomClip RandomClip;

    protected override void Start() {
        base.Start();
        muzzle = GetComponentInChildren<PlayShootingEffects>();
        if(muzzle != null) {
            muzzleTransform = muzzle.transform;
        }
        var tempObject = transform.Find("GunSprite");
        if(tempObject != null) {
            var temp = tempObject.gameObject.GetComponent<SpriteRenderer>();
            gunSpriteScl = temp.transform.localScale.x;
            gunSpriteWidth = temp.sprite.bounds.size.x;
        }


    }

    public bool IsShootable = true;

    protected override void Attack() {
        base.Attack();

        if (!IsShootable) return;

        if (!canShoot) return;

        var shotRecoil = Random.Range(-recoil, recoil);

        var hit = Physics2D.Raycast(Util.Vec3ToVec2(muzzleTransform.position - muzzleTransform.right.normalized * (gunSpriteWidth * gunSpriteScl)),
            Util.Vec3ToVec2(Quaternion.AngleAxis(shotRecoil, Vector3.back) * muzzleTransform.right.normalized), maxShotRange, layerMask);

        muzzle.Shoot(hit, shotRecoil);

        if (hit.collider) {
            hit.collider.gameObject.GetComponent<HealthBase>()?.Damage(damage);
        }

        if(RandomClip != null) {
            RandomClip.Play();
        }

        canShoot = false;
    }
}
