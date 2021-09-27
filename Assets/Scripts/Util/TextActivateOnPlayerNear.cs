using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextActivateOnPlayerNear : MonoBehaviour
{

    public float Radius;
    public bool DisappearOnPlayerLeave;
    public bool ActivateOnce;

    public GameObject TextObject;


    private bool activatedAlready = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Awake() {
        TextObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

        bool playerWithin = Radius * Radius > (Player.instance.transform.position - this.transform.position).sqrMagnitude;

        if (DisappearOnPlayerLeave && Activated && !playerWithin) {
            Deactivate();
        } else if(playerWithin){
            if(!(ActivateOnce && activatedAlready)) {
                Activate();
            }
        }



    }

    private bool Activated = false;

    void Deactivate() {
        Activated = false;
        TextObject.SetActive(false);
    }

    void Activate() {
        if (ActivateOnce && activatedAlready) {
            return;
        }

        Activated = true;
        activatedAlready = true;
        TextObject.SetActive(true);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position, Radius);
    }
}
