using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug;

using static Navmap2D;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

//Optimization ideas
//Seperate nodes into autogenerate bounds boxes (R trees)
//
public class Navmap2DGenerator : MonoBehaviour {



    public static MapData GenerateMap(Navmap2D navmap) {

        var platformsDict = new Dictionary<Vector3Int, Platform>();
        var mapNodes = new Dictionary<Vector3Int, MapNode>();
        var tileMap = navmap.Pathmap;

        int agentHeight = Mathf.Max(1, navmap.agentSettings.GridHeight);



        Debug.Log("Starting generation process");
        Stopwatch watch = new Stopwatch();
        watch.Start();



        bool changePlatforms = true;
        Platform currentPlatform = new Platform();


        List<Vector3Int> currentNodes = new List<Vector3Int>();
        for (int y = tileMap.cellBounds.yMin; y < tileMap.cellBounds.yMax; y++) {

            changePlatforms = true;

            for (int x = tileMap.cellBounds.xMin; x < tileMap.cellBounds.xMax; x++) {
                Vector3Int pos = (new Vector3Int(x, y, (int)tileMap.transform.position.y));

                if (changePlatforms) {
                    changePlatforms = false;

                    if (currentNodes.Count != 0) {

                        currentPlatform.nodes = currentNodes.ToArray();
                        currentPlatform.position = currentPlatform.nodes[0];

                        //Debug.Log("Creating platform at " + currentPlatform.position);

                        currentNodes = new List<Vector3Int>();



                        platformsDict.Add(currentPlatform.position, currentPlatform);
                        currentPlatform = new Platform();

                    }


                }



                if (tileMap.HasTile(pos) && HasSpaceAbove(tileMap, agentHeight, pos)) {
                    MapNode node = new MapNode() {
                        position = pos + new Vector3Int(0, 1, 0),
                        platform = currentNodes.Count > 0 ? currentNodes[0] : pos + new Vector3Int(0, 1, 0),
                        platformIndex = currentNodes.Count
                    };

                    currentNodes.Add(node.position);

                    mapNodes.Add(node.position, node);
                } else {
                    changePlatforms = true;
                }


            }


        }

        List<MapNode> newNodes = new List<MapNode>();

        foreach(var from in mapNodes) {
            List<Vector3Int> nodeConnections = new List<Vector3Int>();
            List<float> nodeDistances = new List<float>();


            foreach(var to in mapNodes) {

                if (to.Value.platform == from.Value.platform) continue;
                if (CanReachObject(navmap, from, to)) {
                    nodeConnections.Add(to.Value.position);
                    nodeDistances.Add(Vector3Int.Distance(from.Value.position, to.Value.position));
                }
            }


            MapNode node = from.Value;

            node.connections = nodeConnections.ToArray();
            node.hasConnections = nodeConnections.Count > 0;
            node.distance = nodeDistances.ToArray();
            newNodes.Add(node);
        }

        mapNodes.Clear();
        for (int i = 0; i < newNodes.Count; i++) {
            mapNodes.Add(newNodes[i].position, newNodes[i]);
        }
        newNodes.Clear();
        foreach(var from in mapNodes) {
            //Check left
            MapNode node = from.Value;
            

            Platform platform = platformsDict[node.platform];
            for (int x = node.platformIndex - 1; x >= 0; x--) {
                MapNode temp = mapNodes[platform.nodes[x]];

                if (temp.hasConnections) {
                    node.hasConnectionL = true;
                    node.connectionL = temp.position;
                    break;
                }
            }

            for (int x = node.platformIndex + 1; x < platform.nodes.Length; x++) {
                MapNode temp = mapNodes[platform.nodes[x]];

                if (temp.hasConnections) {
                    node.hasConnectionR = true;
                    node.connectionR = temp.position;
                    break;
                }
            }

            newNodes.Add(node);
        }


        mapNodes.Clear();

        for(int i = 0; i < newNodes.Count; i++) {
            mapNodes.Add(newNodes[i].position, newNodes[i]);
        }


        watch.Stop();
        Debug.Log("Finished generation process. Process took: " + watch.ElapsedMilliseconds + "ms");
        Debug.Log((tileMap.cellBounds.size.x * tileMap.cellBounds.size.y) + " Scanned tiles");
        Debug.Log(mapNodes.Values.Count + " Map Nodes Created");
        Debug.Log(platformsDict.Values.Count + " Platforms Created");


        return new MapData() {
            platforms = platformsDict,
            nodes = mapNodes
        };

    }

    /// <summary>
    /// Inclusive take on check if a tile is persisitant from startPos to end startPos + amount
    /// </summary>
    /// <param name="map"></param>
    /// <param name="amount"></param>
    /// <param name="startPos"></param>
    public static bool HasSpaceAbove(Tilemap map, int amount, Vector3Int startPos) {
        for (int i = 0; i < amount; i++) {
            if (map.HasTile(new Vector3Int(0, i + 1, 0) + startPos)) {
                return false;
            }
        }

        return true;
    }


    //Rules to implement
    //Check along the raycast (A* you way there or something) to see if you can fit through where ever you need to go (anti crouch)
    
    //Add trajectory check (Have the node shoot multiple trajectories to see if they land or not at the required node or on the node platform
    //For the above you need to generate platforms before you connect the jump nodes

    //Check if drop down is available
    //Raycast via boxWidth???? (aka 2d box cast??)
    
    public static bool CanReachObject(Navmap2D navmap, KeyValuePair<Vector3Int, MapNode> from, KeyValuePair<Vector3Int, MapNode> to) {

        for(int x = -1; x < 2; x++) {
            for (int y = -1; y < 2; y++) {
                //Debug.Log(x + " " + y);

                for (int x2 = -1; x2 < 2; x2++) {
                    for (int y2 = -1; y2 < 2; y2++) {

                        Vector3 a = navmap.Pathmap.CellToWorld(from.Key) + (new Vector3(x, y, 0) * 0.5f) + navmap.offset;
                        Vector3 b = navmap.Pathmap.CellToWorld(to.Key) + (new Vector3(x2, y2, 0) * 0.5f) + navmap.offset;

                        Vector3 dir = b - a;
                        float distance = dir.sqrMagnitude;
                        //Debug.DrawLine(a, a + new Vector3(0, 0.1f), Color.blue, 2);

                        if (distance > navmap.agentSettings.maxJumpRadius * 2) {
                            continue;
                        }

                        //Debug.DrawRay(a, dir, Color.yellow, 1);
                        if (Physics2D.Raycast(a, dir.normalized, distance, navmap.collisionMask)) {

                        } else {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Navmap2D))]
public class Navmap2DGeneratorEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        if (GUILayout.Button("Generate")) {
            Navmap2D navmap = (Navmap2D)target;
            navmap.data = Navmap2DGenerator.GenerateMap(navmap);
            navmap.SaveDataEditor(EditorSceneManager.GetActiveScene().path.Replace(".unity", ""));

            EditorUtility.SetDirty(navmap);
            EditorSceneManager.MarkSceneDirty(navmap.gameObject.scene);

        }
    }
}
#endif



