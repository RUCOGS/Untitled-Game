using UnityEngine;

public class TntExploder : WorldItemEffector, IManualInteract {
    
    void Start() {
        canBeManipulated = true;
    }

    public override void Use() {
        base.Use();
        canBeManipulated = false;
    }
    
    public void Pickup() {
        
    }



}
