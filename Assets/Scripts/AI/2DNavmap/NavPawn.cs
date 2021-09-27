using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavPawn : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public Transform target;
    public Navmap2D navmap;

    public Navmap2DPath path;

    public bool findPath;

    public Navmap2D.MapNode currentTileData;

    // Update is called once per frame
    void Update()
    {
        if (findPath) {
            //findPath = false;
            path = navmap.FindPath(this.transform.position, target.position);

        }
        navmap.data.nodes.TryGetValue(navmap.Pathmap.WorldToCell(transform.position), out currentTileData);
    }



    [ContextMenu("Find path")]
    public void FindPath() {
        path = navmap.FindPath(this.transform.position, target.position);
    }

    private void OnDrawGizmos() {

        Vector3Int posC = navmap.Pathmap.WorldToCell(transform.position);
        Vector3Int targetC = navmap.Pathmap.WorldToCell(target.position);
        

        
        

        Gizmos.DrawCube(navmap.Pathmap.CellToWorld(posC) + navmap.offset, Vector3.one * 0.5f);
        Gizmos.DrawCube(navmap.Pathmap.CellToWorld(targetC) + navmap.offset, Vector3.one * 0.5f);

        if (path != null) {
            
            bool first = true;
            Vector3 lastPos = Vector3Int.zero;

            var newStack = new Stack<Navmap2D.MapNode>();

            while(path.path.Count > 0) {
                var node = path.path.Pop();

                var curPos = navmap.Pathmap.CellToWorld(node.position);


                Gizmos.color = Color.red;
                Gizmos.DrawSphere(curPos + navmap.offset, 0.25f);
                if (!first) {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(curPos + navmap.offset, lastPos + navmap.offset);
                }


                first = false;

                lastPos = curPos;


                newStack.Push(node);
            }


            while(newStack.Count > 0) {
                path.path.Push(newStack.Pop());
            }
        } else {
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(this.transform.position, Vector3.one * 0.5f);
        }
    }
}
