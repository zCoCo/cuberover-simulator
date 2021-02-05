using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;

public class GameObjectPool {
    Queue<GameObject> q;
    HashSet<GameObject> dePooled;
    public HashSet<GameObject> collection;

    public GameObjectPool() {
        dePooled = new HashSet<GameObject>();
        q = new Queue<GameObject>();
        collection = new HashSet<GameObject>();
    }

    public GameObjectPool(Collection<GameObject> collection) {
        q = new Queue<GameObject>();
        this.collection = new HashSet<GameObject>();

        foreach (GameObject obj in collection) {
            AddToPool(obj);
        }
    }

    public void AddToPool(GameObject obj) {
        obj.SetActive(false);
        q.Enqueue(obj);
        collection.Add(obj);
    }

    public GameObject DePool() {
        GameObject obj = q.Dequeue();
        obj.SetActive(true);
        dePooled.Add(obj);
        return obj;
    }

    public bool IsPooled(GameObject obj) {
        return q.Contains(obj);
    }

    /// <summary>
    /// Put an object back to pool.
    /// </summary>
    /// <param name="obj"></param>
    public void Pool(GameObject obj) {
        if (obj && collection.Contains(obj)) {
            q.Enqueue(obj);
            obj.SetActive(false);
            dePooled.Remove(obj);
        } else {
            Debug.LogWarning("You are trying to return an object that does not belong to the pool collection.");
        }
    }

    /// <summary>
    /// Pool all objects in this pool
    /// </summary>
    public void PoolAll() {
        foreach(var obj in collection) {
            if (!IsPooled(obj)) {
                Pool(obj);
            }
        }
    }

    /// <summary>
    /// A collection of all depooled objects.
    /// </summary>
    /// <returns></returns>
    public HashSet<GameObject> DePooledObjects() {
        return dePooled;
    }

    /// <summary>
    /// Destroy the game objects in this pool
    /// </summary>
    public void Discard() {
        foreach(GameObject go in q) {
            Object.DestroyImmediate(go);
        }
    }
}

//public class ObjectPool<T> {
//    Queue<T> q;
//    HashSet<T> collection;

//    public ObjectPool() {
//        q = new Queue<T>();
//        collection = new HashSet<T>();
//    }

//    public ObjectPool(Collection<T> collection) {
//        q = new Queue<T>();
//        this.collection = new HashSet<T>();

//        foreach (T obj in collection) {
//            AddToPool(obj);
//        }
//    }

//    public void AddToPool(T obj) {
//        q.Enqueue(obj);
//        collection.Add(obj);
//    }

//    public T DePool() {
//        return q.Dequeue();
//    }

//    public bool IsPooled(T obj) {
//        return q.Contains(obj);
//    }

//    /// <summary>
//    /// Put an object back to pool.
//    /// </summary>
//    /// <param name="obj"></param>
//    public void Pool(T obj) {
//        if (collection.Contains(obj)) {
//            q.Enqueue(obj);
//        } else {
//            Debug.LogWarning("You are trying to return an object that does not belong to the pool collection.");
//        }
//    }

//}