using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System.Runtime.CompilerServices;
using UnityEngine.Experimental.Rendering.Universal;

public class GridShadowCastersGenerator : MonoBehaviour {
#if UNITY_EDITOR
    public string colliderTag = "GenerateShadowCasters";
    public GameObject shadowCasterPrefab;
    public Transform shadowCastersContainer;
    public bool removePreviouslyGenerated = true;


    private List<Vector3> availablePlaces;
    private List<GameObject> objectsCreated = new  List<GameObject>();

    private int currentShadowCaster = 0;

    public GameObject[] Generate() {
        Debug.Log("### Generating ShadowCasters ###");

        /* get the bounds of the area to check */

        // collect colliders specified by tag

        if (removePreviouslyGenerated) {
            for (int i = shadowCastersContainer.childCount; i > 0; --i) {
                GameObject child = shadowCastersContainer.GetChild(i - 1).gameObject;
                Undo.DestroyObjectImmediate(child.gameObject);
            }
        }
        Undo.SetCurrentGroupName("Generate Shadows");

        var colliders = new List<Tilemap>();
        var tagedGos = GameObject.FindGameObjectsWithTag(colliderTag);
        currentShadowCaster = 0;

        foreach (var go in tagedGos) {
            var goColliders = go.GetComponents<Tilemap>();

            foreach (var goc in goColliders) {
               colliders.Add(goc);
            }
        }

        int continueLength = 0;
        Vector3 currentPos = Vector3.zero;
        foreach (var tileMap in colliders) {
            availablePlaces = new List<Vector3>();

            for (int n = tileMap.cellBounds.yMin; n < tileMap.cellBounds.yMax; n++) {
                for (int p = tileMap.cellBounds.xMin; p < tileMap.cellBounds.xMax; p++) {
                    Vector3Int localPlace = (new Vector3Int(p, n, (int)tileMap.transform.position.y));
                    Vector3 place = tileMap.CellToWorld(localPlace);
                    if (tileMap.HasTile(localPlace)) {
                        //Tile at "place"
                        if(continueLength == 0) {
                            currentPos = place;
                        }
                        continueLength++;
                    } else {
                        //No tile at "place"
                        if(continueLength > 0) {
                            GenerateShadowObject(currentPos, continueLength);
                            continueLength = 0;
                        }

                    }
                }
                GenerateShadowObject(currentPos, continueLength);
                continueLength = 0;
            }

        }



        return objectsCreated.ToArray();
    }

    void GenerateShadowObject(Vector3 firstPosition, int continueLength) {
        Vector3 newPos = firstPosition;
        newPos.x = firstPosition.x + (continueLength * 0.5f * 0.5f) - 0.25f;
        GameObject prefabObj = (GameObject)PrefabUtility.InstantiatePrefab(shadowCasterPrefab, shadowCastersContainer);
        Undo.RegisterCreatedObjectUndo(prefabObj, Undo.GetCurrentGroupName());
        prefabObj.transform.position = newPos + new Vector3(0.5f * shadowCasterPrefab.transform.localScale.x, 0.5f * shadowCasterPrefab.transform.localScale.y);
        prefabObj.transform.localScale = new Vector3(prefabObj.transform.localScale.x * (continueLength), prefabObj.transform.localScale.y);
        prefabObj.name = "shadow_caster_" + currentShadowCaster;
        currentShadowCaster++;
        objectsCreated.Add(prefabObj);
    }
    

    bool IsHit(Vector2 pos) {
        var margin = .2f; // prevents overlapping

        // get tile bounds

        var bottomLeft = new Vector2(pos.x - 0.5f + margin, pos.y + 0.5f - margin);
        var topRight = new Vector2(pos.x + 0.5f - margin, pos.y - 0.5f + margin);

        //check for collisions

        Collider2D[] colliders = Physics2D.OverlapAreaAll(bottomLeft, topRight);

        foreach (var col in colliders) {
            if (col.CompareTag(colliderTag)) {
                return true;
            }
        }

        return false;
    }
#endif



    
}

#if UNITY_EDITOR
[CustomEditor(typeof(GridShadowCastersGenerator))]
public class GridShadowCastersGeneratorEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        if (GUILayout.Button("Generate")) {
            var generator = (GridShadowCastersGenerator)target;
            var casters = generator.Generate();


        }
    }
}
#endif
