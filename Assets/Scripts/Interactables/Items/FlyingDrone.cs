using UnityEngine;

public class FlyingDrone : WorldEffectedItem {
    private bool up = false;
    public override void Effect() {
        transform.position += Vector3.up * (up ? -3 : 3);
        up = !up;
    }
}
