using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.UIElements;


//This will be used as the basic structure of a single navmap2D
//You will be able to use generateNavmap script to generate a new navmap/update a original navmap

//Dedicated to ground enemys, for Air enemies the tilemap will be used in a A* algorithm


//Parts of an navmap include
//Nodes (These are tiles in the world where a path is doable)
//Platform node (This node connects with other nodes side by side saying this is a platform you can wander on)
//Jump node (This node tells the ai at this point inorder to reach a different platform you must jump) - Found after platform ends
//Jump nodes do not include connect jumpnodes via platform jumpnodes
//Jump nodes are on the last edge of the platform instead after the edge

//Jump nodes are connected to other jump nodes indicating where this can lead

//Detection
//Work will be destributed

//1st Tilemap(s) will be dissected for their tiles and location of their tiles, and data will be stored in hashmap (Vector2Int)
//A single grid will be used to determine the world location of these tilemaps (Vector2Int will be determined in local space)

//2nd Tile maps are then divided up to be processed by job system

//3nd Processing will do the following
//Determine if the current node (by checking its neighboors for occupation) is a platform node (will also accomadate a custom height variable i.e. how many tiles tall is the agent && how wide is the agent [Only for enclosed areas on the side])
//Second stage of processing
//Takes all platform nodes and see if their sides contain no tile
//If their sides contain no tile, then make it sumbit a jumpnode to that location and link it to the current node

//4th all nodes are then connected with their neighboor platform nodes/jump nodes (node a touches b and c, so a connects with b and c)

//Processing jump nodes
//Jump nodes will be processed in the following matter
//Job system to determine close enough neighboors to consider for process
//Critia are as follows
//Maximum distance

//With each jump node having neighboors ) solved for, then a single thread will perform checks via raycasting to determined if LOS is blocked with the node
//If LOS is not blocked and/or via simple check if its a jump up (L shape jump with blocks being the L) then it will connect with the node

//After processing nodes, they are given a weight



//PATHFINDING
//Pathfinding will be a simple solving of a 2nd stage process
//Determine the current node/nearest node the enemy is on and the place it wants to go
//If no nodes are found for the above, then it cant move anywhere

//If nodes are found:
//Use A star/Dijkstra/BreadthFirst to determine the path to take amoung nodes

//Send agent path

//Edge cases
//Have pathfinding work as a job, for every request, schedule a job
//When the job is complete the AI will have an update path sent to its variable where the navmap agent can react
//Paths are not guranteed within x amount of frames (unless an option would be set)
//If tracking the player, have the nav agent continue along its path until it reaches the end of its platform, (if nav path is not recieved) 
/// <summary>
/// Contains data and helper functions of navigation points and platforms for a sidescrolling platformer
/// </summary>
public class Navmap2D : MonoBehaviour
{

    public LayerMask collisionMask;
    //[HideInInspector]
    public TextAsset dataAssetPlatforms;
    public TextAsset dataAssetNodes;
    public TextAsset dataAsset;
    public MapData data;
    public Tilemap Pathmap;
    public AgentSettings agentSettings;
    public Vector3 offset = new Vector3(0.25f, 0.25f);
    [Serializable]
    public struct AgentSettings {
        public float maxJumpRadius;
        public int GridHeight;
        public int GridWidth;

    }

    [System.Serializable]
    public struct Platform {
        //X is located for the most left node
        public Vector3Int position;

        //Should be ordered from left to right of the platform
        public Vector3Int[] nodes;
    }

    [System.Serializable]
    public struct MapNode {
        public Vector3Int position;
        public Vector3Int platform;

        public int platformIndex;

        public bool hasConnections;
        //Nodes
        public Vector3Int[] connections;
        public float[] distance;


        public bool hasConnectionL;
        public bool hasConnectionR;

        public Vector3Int connectionL, connectionR;
    }

    [System.Serializable]
    public struct MapData {

        public Dictionary<Vector3Int, Platform> platforms;
        public Dictionary<Vector3Int, MapNode> nodes;
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadData();

        foreach(var node in data.nodes) {
            if(node.Value.connections != null) {

                for(int i = 0; i < node.Value.connections.Length; i++) {
                    Debug.Log(node.Value.connections[i]);
                }
            } else {
                Debug.Log("It did not work ;-;");
            }

            break;
        }

        pathFinder = GetComponent<Navmap2DAStar>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LoadData() {
        var sPlatforms = JsonConvert.DeserializeObject<Platform[]>(dataAssetPlatforms.text);
        var sNodes = JsonConvert.DeserializeObject<MapNode[]>(dataAssetNodes.text);

        data.platforms = new Dictionary<Vector3Int, Platform>();
        data.nodes = new Dictionary<Vector3Int, MapNode>();

        foreach (var platform in sPlatforms) {
            data.platforms.Add(platform.position, platform);
        }
        foreach (var node in sNodes) {
            data.nodes.Add(node.position, node);
        }

    }

#if UNITY_EDITOR
    public void SaveDataEditor(string Pathfile) {
        //serialize.IgnoreSystemAndUnitySerializeAttributes = true;
        string fileName = "/NavMap/agent1/";
        //Debug.Log(JSON.Serialize(data, serialize).CreatePrettyString());
        TextAsset text_platforms = new TextAsset(JsonConvert.SerializeObject(data.platforms.Values));
        TextAsset text_nodes = new TextAsset(JsonConvert.SerializeObject(data.nodes.Values));


        (new FileInfo(Pathfile + fileName)).Directory.Create();
        UnityEditor.AssetDatabase.CreateAsset(text_platforms, Pathfile + fileName + "platforms.txt");
        //UnityEditor.AssetDatabase.CreateAsset(dataAsset, Pathfile + fileName + "platforms.txt");
        UnityEditor.AssetDatabase.CreateAsset(text_nodes, Pathfile + fileName + "nodes.txt");
        dataAssetPlatforms = text_platforms;
        dataAssetNodes = text_nodes;

        //Debug.Log(JsonConvert.SerializeObject(data, Formatting.Indented));
        //Debug.Log(JsonConvert.SerializeObject(data.nodes, Formatting.Indented));
        //Debug.Log(JsonConvert.SerializeObject(data.nodes.Values, Formatting.Indented));
        //Debug.Log(JsonConvert.SerializeObject(data.nodes.Values.ToArray(), Formatting.Indented));

        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
        LoadData();
    }
#endif

    public bool RenderNodes;

    private void OnDrawGizmosSelected() {
        if(data.platforms == null) {
            LoadData();
            return;
        }

        foreach(var plat in data.platforms) {
            Platform platform = plat.Value;

            if (platform.nodes == null) { continue; }
            for(int n = 0; n < platform.nodes.Length; n++) {

                Vector3 currentPos = Pathmap.CellToWorld(platform.nodes[n]) + offset;
                if(n == 0) {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(currentPos, 0.25f);
                }
                Gizmos.color = Color.white;
                Gizmos.DrawWireCube(currentPos, new Vector3(0.5f, 0.5f, 0.5f));

                MapNode node;
                if(data.nodes.TryGetValue(platform.nodes[n], out node)) {

                    if (node.hasConnections) {
                        foreach (var connection in node.connections) {
                            Vector3 toPos = Pathmap.CellToWorld(connection) + offset;
                            Gizmos.color = Color.magenta;

                            Gizmos.DrawLine(currentPos, toPos);
                        }
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawWireSphere(currentPos, 0.2f);
                    }
                } else {
                    Debug.Log("What");
                }

                if (n == platform.nodes.Length - 1) {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(currentPos, 0.15f);
                }
            }
        }
    }


    public Navmap2DAStar pathFinder;


    public Navmap2DPath FindPath(Vector3 pos, Vector3 target) {
        Vector3Int posC = Pathmap.WorldToCell(pos);
        Vector3Int targetC =  Pathmap.WorldToCell(target);
        MapNode fromN, toN;

        if (data.platforms == null) {
            LoadData();
        }

        if (data.nodes.TryGetValue(posC, out fromN) && data.nodes.TryGetValue(targetC, out toN)) {

            if(pathFinder == null) {
                pathFinder = GetComponent<Navmap2DAStar>();
            }

            return pathFinder.FindPath(this, fromN, toN);
        } else {

            Debug.Log("Could not find valid node at position");
            return new Navmap2DPath();
        }


    }

    public Vector3 CellToWorld(Vector3Int cellPos) {
        return Pathmap.CellToWorld(cellPos) + offset;
    }

}

