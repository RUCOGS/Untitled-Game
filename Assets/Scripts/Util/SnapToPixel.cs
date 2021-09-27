using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapToPixel : MonoBehaviour
{

    public int PPU;


    
    private void LateUpdate() {
        Vector3 pos = this.transform.position;
        float unit = (1.0f / (float)PPU);
        pos.x = pos.x % unit;
        pos.y = pos.y % unit;
        pos.z = pos.z % unit;


        pos.x = pos.x > unit/2 ? pos.x + unit : pos.x;
        pos.y = pos.y > unit/2 ? pos.y + unit : pos.y;
        pos.z = pos.z > unit/2 ? pos.z + unit : pos.z;


        Debug.Log(pos.x % unit);
        Debug.Log(unit);
        pos = this.transform.position - pos;

        this.transform.position = pos;
    }
}
