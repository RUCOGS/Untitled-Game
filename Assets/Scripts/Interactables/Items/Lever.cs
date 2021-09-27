using UnityEngine;

public class Lever : WorldItemEffector, IManualInteract {
    public Sprite right;
    public Sprite left;

    public bool singleUse;
    
    public SpriteRenderer sr;
    
    private bool isRight = true;

    private void Start() {
        canBeManipulated = true;
    }

    public override void Use() {
        base.Use();
        sr.sprite = isRight ? left : right;
        isRight = !isRight;
        canBeManipulated = !singleUse;
    }

    public void Pickup() {

    }
}
