using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using static Navmap2D;

public class Navmap2DAStar : MonoBehaviour, IPathfinder
{

    private Coroutine currentCor;
    private Navmap2D currentNavmap;

    public Navmap2DPath EEE(Navmap2D navmap, MapNode from, MapNode to) {
        currentNavmap = navmap;
        if (currentCor != null) {
            StopCoroutine(currentCor);
            currentCor = null;
        }
        //currentCor = StartCoroutine(FindPathCoroutine(navmap, from, to));
        return new Navmap2DPath();
    }

    private void OnDrawGizmos() {
        if(currentCor == null) {
            return;
        }

        Gizmos.color = Color.green;
        foreach (var key in open.Keys) {
            Gizmos.DrawCube(currentNavmap.CellToWorld(key), Vector3.one * 0.25f);
        }
        Gizmos.color = Color.red;
        foreach (var key in closed.Keys) {
            Gizmos.DrawCube(currentNavmap.CellToWorld(key), Vector3.one * 0.25f);
        }
    }

    private Dictionary<Vector3Int, AStarNode> open = new Dictionary<Vector3Int, AStarNode>();
    private Dictionary<Vector3Int, AStarNode> closed = new Dictionary<Vector3Int, AStarNode>();

    public Navmap2DPath FindPath(Navmap2D navmap, MapNode from, MapNode to) {

        open.Clear();
        closed.Clear();


        open.Add(from.position, new AStarNode() { 
            totalCost = 0,
            movementCost = 0,
            distanceToFinish = 0,
            parent = from.position,
            current = from.position
        });

        int iterations = 0;
        bool found = false;
        AStarNode endNode = new AStarNode();

        while(!found || open.Count > 0) {
            iterations++;
            //Find lowest total cost node in open

            bool first = true;
            AStarNode lowestFNode = new AStarNode();
            int i;

            foreach(var aNode in open.Values) {
                if (first) {
                    lowestFNode = aNode;
                    first = false;
                    continue;
                }

                if (lowestFNode.totalCost > aNode.totalCost) {
                    lowestFNode = aNode;
                }
            }

            open.Remove(lowestFNode.current);

            MapNode mNode;

            if(navmap.data.nodes.TryGetValue(lowestFNode.current, out mNode)) {

            } else {
                Debug.Log("Could not find a node on iteration #" + iterations);
                break; 
            }

            if (lowestFNode.current == to.position) {
                //Found it

                found = true;
                endNode = lowestFNode;
                break;
            }

            //Generate successors
            var succesors = new List<AStarNode>();
            if (mNode.hasConnections) {
                for (i = 0; i < mNode.connections.Length; i++) {
                    succesors.Add(GenerateNode(mNode.connections[i], mNode.position, to.position));
                }
            }

            if (mNode.hasConnectionL) {
                AStarNode newNode = GenerateNode(mNode.connectionL, mNode.position, to.position);
                succesors.Add(newNode);
            }

            if (mNode.hasConnectionR) {
                AStarNode newNode = GenerateNode(mNode.connectionR, mNode.position, to.position);
                succesors.Add(newNode);
            }

//Succesor logic

            for (i = 0; i < succesors.Count; i++) {

                AStarNode curSuccessor = succesors[i];


                curSuccessor.movementCost = lowestFNode.movementCost + curSuccessor.movementCost;

                curSuccessor.totalCost = curSuccessor.movementCost + curSuccessor.distanceToFinish;

                AStarNode foundNode;

                if (open.TryGetValue(curSuccessor.current, out foundNode)) {
                    if (foundNode.totalCost <= curSuccessor.totalCost) {
                        continue;
                    }
                }else 

                if (closed.TryGetValue(curSuccessor.current, out foundNode)) {
                    if(foundNode.totalCost <= curSuccessor.totalCost) {
                        continue;
                    }

                    //closed.Remove(foundNode.current);
                    //open.Add(foundNode.current, foundNode);
                } else {

                    open.Add(curSuccessor.current, curSuccessor);

                }

                //yield return null;
            }

            closed.Add(lowestFNode.current, lowestFNode);
            //yield return null;
        }

        Navmap2DPath navPath = new Navmap2DPath();

        if (found) {

            AStarNode curNode = endNode;
            while(curNode.current != from.position) {
                navPath.path.Push(navmap.data.nodes[curNode.current]);


                AStarNode parentNode;
                if (closed.TryGetValue(curNode.parent, out parentNode)) {
                    curNode = parentNode;
                } else if(open.TryGetValue(curNode.parent, out parentNode)) {
                    curNode = parentNode;
                } else {
                    Debug.LogError("Error: Trying to find path failed. Could not find parrent of AStarNode");
                    break;
                }
            }

        }

        return navPath;

    }

    private AStarNode GenerateNode(Vector3Int node, Vector3Int parent, Vector3Int goal) {
        AStarNode aNode = new AStarNode();
        
        aNode.parent = parent;
        aNode.current = node;

        aNode.distanceToFinish = (parent-goal).sqrMagnitude;
        aNode.movementCost = (parent-node).sqrMagnitude;
        aNode.totalCost = aNode.movementCost + aNode.distanceToFinish;

        return aNode;
    }



    

    private struct AStarNode {
        public float totalCost;
        public float movementCost;
        public float distanceToFinish;
        public Vector3Int parent;
        public Vector3Int current;
    }



}
