using UnityEngine;

public class PlayShootingEffects : MonoBehaviour {

    public GameObject bulletImpact;
    public float bulletImpactTime;
    public GameObject bulletTrail;
    public float bulletTrailTime;
    public int numRndMuzzleFlashes;

    public Color defaultColor;
    public Color enemyColor;
    
    private Animator flashAnimation;

    private int fireHash;
    private int flashRndHash;

    private int defaultLayer;
    private int enemyLayer;

    private float shotDist;
    
    private void Start() {
        flashAnimation = GetComponent<Animator>();
        fireHash = Animator.StringToHash("Fire");
        flashRndHash = Animator.StringToHash("RndAnim");

        defaultLayer = LayerMask.NameToLayer("Default");
        enemyLayer = LayerMask.NameToLayer("Enemy");

        shotDist = GetComponentInParent<Gun>().maxShotRange;
    }

    public void Shoot(RaycastHit2D hit, float shotRecoil) {
        flashAnimation.SetInteger(flashRndHash, Random.Range(0, numRndMuzzleFlashes));
        flashAnimation.SetTrigger(fireHash);

        var newLr = Instantiate(bulletTrail);
        newLr.GetComponent<LineRenderer>().SetPositions(new[] {
            transform.position,
            hit.collider
                ? Util.Vec2ToVec3(hit.point)
                : Quaternion.AngleAxis(shotRecoil, Vector3.back) * transform.right.normalized * shotDist + transform.position
        });
        Destroy(newLr, bulletTrailTime);

        if (!hit.collider) return;
        
        var newBi = Instantiate(bulletImpact);
        newBi.transform.position = hit.point;
        newBi.transform.right = hit.normal;

        var newBiMain = newBi.GetComponent<ParticleSystem>().main;

        if (hit.transform.gameObject.layer == defaultLayer) {
            newBiMain.startColor = defaultColor;
        }
        else if (hit.transform.gameObject.layer == enemyLayer) {
            newBiMain.startColor = enemyColor;
        }

        Destroy(newBi, bulletImpactTime);
    }
}
