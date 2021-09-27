using System.Linq;
using UnityEngine;

public class BulletTrail : MonoBehaviour {
    
    private LineRenderer lr;
    public float trailSpeed;

    private Vector3 start;
    private Vector3 end;
    
    private void Start() {
        lr = gameObject.GetComponent<LineRenderer>();
        start = lr.GetPosition(0);
        end = lr.GetPosition(1);
        lr.endColor = Random.ColorHSV(.00f, .25f, 1f, 1f, 1f, 1f, .33f, .33f);
    }

    private void FixedUpdate() {
        lr.SetPosition(0, Vector3.MoveTowards(start, end, trailSpeed));
        start = lr.GetPosition(0);
    }
}
