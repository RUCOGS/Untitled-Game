using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseDoor : WorldEffectedItem {

    public SpriteRenderer doorSr;
    public int steps;
    
    public override void Effect() {
        StartCoroutine(LowerDoor());
    }

    IEnumerator LowerDoor() {
        for (int i = 0; i < steps; i++) {
            transform.position -= new Vector3(0,(doorSr.size.y * transform.localScale.y) / steps, 0);
            yield return null;
        }
    }
}
