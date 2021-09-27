using UnityEngine;

public class Util : MonoBehaviour
{
    public static Vector2 Vec3ToVec2(Vector3 vec) {
        return new Vector2(vec.x, vec.y);
    }
    
    public static Vector3 Vec2ToVec3(Vector2 vec) {
        return new Vector3(vec.x, vec.y, 0);
    }
}
