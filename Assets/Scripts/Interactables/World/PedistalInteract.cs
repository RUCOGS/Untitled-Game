using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PedistalInteract : WorldItemEffector, IManualInteract {

    public GameObject crystal;
    private bool activated = false;

    public override bool CanUse() {

        if (activated || Player.instance.inventory.equippedWeapon == null) return false;

        return Player.instance.inventory.equippedWeapon.tag == "crystal";

    }


    public override void Use() {
        base.Use();
        crystal.SetActive(true);
        activated = true;
        Player.instance.inventory.ConsumeCurrentlyHeld();
        
    }
}
