using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif 
public abstract class PoolBehaviour : MonoBehaviour
{
    public bool UseSharedPool;
    public string SharedPoolName;


    private void Awake() {
        if (UseSharedPool) {

            if(SharedPoolName == "") {
                SharedPoolName = poolablePrefab.name;
            }

            SharedPool pool;

            if(SharedPools.TryGetValue(SharedPoolName, out pool)) {
                pool.active++;

            } else {
                pool = new SharedPool();
                pool.active++;

                SharedPools.Add(SharedPoolName, pool);
            }



        }
    }

    public static Dictionary<string, SharedPool> SharedPools = new Dictionary<string, SharedPool>();

    Stack<IPoolable> pooledObjects = new Stack<IPoolable>();
    public GameObject poolablePrefab;

    public void PoolObject(IPoolable poolable) {

        if (UseSharedPool) {

            poolable.Pool();
            //Try using a cached version of the pool
            SharedPools[SharedPoolName].pool.Push(poolable);

            return;
        }


        poolable.Pool();

        pooledObjects.Push(poolable);

    }

    public IPoolable RetrieveObject() { 

        if (UseSharedPool) {
            IPoolable sPoolable;

            SharedPool poolShared = SharedPools[SharedPoolName];
            if (poolShared.pool.Count == 0) {
                GameObject obj = Instantiate(poolablePrefab);
                sPoolable = obj.GetComponent<IPoolable>();

                sPoolable.poolParent = this;
            } else {
                sPoolable = poolShared.pool.Pop();
            }

            sPoolable.Unpool();
            //Try using a cached version of the pool

            return sPoolable;
        }



        IPoolable pooled;
        if (pooledObjects.Count == 0) {
            GameObject obj  = Instantiate(poolablePrefab);
            pooled = obj.GetComponent<IPoolable>();

            pooled.poolParent = this;
        } else {
            pooled = pooledObjects.Pop();
        }

        pooled.Unpool();

        return pooled;

    }

    private void OnDestroy() {

        if (UseSharedPool) {

            SharedPool pool;

            if (SharedPools.TryGetValue(SharedPoolName, out pool)) {
                pool.active--;

                if (pool.active == 0) {
                    SharedPools.Remove(SharedPoolName);

                    pool.Destroy();
                }
            }

        } else {
            foreach (var obj in pooledObjects) {
                obj.Clean();
            }
            pooledObjects.Clear();
        }



        
    } 


}

#if UNITY_EDITOR


[CustomEditor(typeof(PoolBehaviour), true)]
public class PoolBehaviorEditor : Editor {


    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        
        if (GUILayout.Button("Debug Print")) {

            foreach (var pair in PoolBehaviour.SharedPools) {

                Debug.Log("[" + pair.Key + "] Active: " + pair.Value.active + " Stored: " + pair.Value.pool.Count);

            }

        }
    }


}

#endif


public class SharedPool {

    public int active;
    public Stack<IPoolable> pool = new Stack<IPoolable>();

    public void Destroy() {
        foreach (var obj in pool) {
            obj.Clean();
        }

        pool.Clear();
    }
}