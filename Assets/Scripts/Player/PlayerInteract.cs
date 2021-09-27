using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Experimental.Rendering.Universal;

public class PlayerInteract : MonoBehaviour {
    public BoxCollider2D playerCollider;

    private Vector2 playerPosition;

    private readonly List<Collider2D> autoCollisions = new List<Collider2D>();
    private readonly List<Collider2D> manualCollisions = new List<Collider2D>();

    public float consumeThresh;

    public float autoRadius;

    public float manualRadius;

    public float indicatorLightIntensity;
    
    private ContactFilter2D itemContactFilter;
    private ContactFilter2D wallContactFilter;

    private Light2D indicatorLight;
    private Transform targetForLight;

    private readonly RaycastHit2D[] walls = new RaycastHit2D[1];

    // Start is called before the first frame update
    void Start() {
        itemContactFilter = new ContactFilter2D();
        itemContactFilter.SetLayerMask(LayerMask.GetMask("Items")); // Layer 9
        itemContactFilter.useLayerMask = true;

        wallContactFilter = new ContactFilter2D();
        wallContactFilter.SetLayerMask(LayerMask.GetMask("Default")); // Layer 1
        wallContactFilter.useLayerMask = true;

        indicatorLight = GetComponent<Light2D>();
    }
    
    /*#if UNITY_EDITOR
    private void OnDrawGizmos() {
        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.DrawWireDisc(playerPosition, Vector3.back, autoRadius);
        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.DrawWireDisc(playerPosition, Vector3.back, consumeThresh);
        UnityEditor.Handles.color = Color.green;
        UnityEditor.Handles.DrawWireDisc(playerPosition, Vector3.back, manualRadius);
    }
    #endif*/

    // Update is called once per frame
    void Update() {
        playerPosition = Util.Vec3ToVec2(playerCollider.transform.position);

        Physics2D.OverlapCircle(playerPosition, autoRadius, itemContactFilter, autoCollisions);

        foreach (Collider2D autoPickupItem in autoCollisions.Where(x => x.gameObject.GetComponent<IAutoPickup>() != null)) {
            if (Vector2.Distance(playerPosition, autoPickupItem.attachedRigidbody.position) < consumeThresh) {
                IAutoPickup inter = autoPickupItem.gameObject.GetComponent<IAutoPickup>();
                inter.Use();
                inter.OnPickup();
            } else {
                autoPickupItem.attachedRigidbody.AddForce((playerPosition - autoPickupItem.attachedRigidbody.position) * 2f);
            }
        }

        Physics2D.OverlapCircle(playerPosition, manualRadius, itemContactFilter, manualCollisions);

        Weapon weapComp = null;
        WorldItemEffector itemComp = null;
        
        var closestItem = manualCollisions.Where(x => x.gameObject.GetComponent<IManualInteract>() != null).ToList()
            .OrderBy(x => Vector2.Distance(Util.Vec3ToVec2(x.transform.position), playerPosition)).ToList()
            .Find(x => {
                weapComp = x.gameObject.GetComponent<Weapon>();
                itemComp = x.gameObject.GetComponent<WorldItemEffector>();
                return (weapComp && !weapComp.equipped || itemComp.CanUse() && itemComp && itemComp.canBeManipulated) &&
                       Physics2D.Linecast(playerPosition, Util.Vec3ToVec2(x.transform.position), wallContactFilter,
                           walls) == 0;
            });

        if (closestItem) {
            if (weapComp) {
                targetForLight = closestItem.transform.Find("GunSprite");
                if(targetForLight == null) {
                    targetForLight = closestItem.transform;
                }
                indicatorLight.intensity = indicatorLightIntensity;
            } else if(itemComp) {
                targetForLight = closestItem.transform;
                indicatorLight.intensity = indicatorLightIntensity;
            }
            
            if (Input.GetKeyDown(KeyCode.E)) {
                var Interactor = closestItem.GetComponent<IManualInteract>();

                Interactor?.Use();

                
            }
        } else {
             targetForLight = null;
             indicatorLight.intensity = 0;
        }
    }

    private void LateUpdate() {
        if(targetForLight != null) {
            indicatorLight.transform.position = targetForLight.position;
        }
    }
}
