using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Iris/Util/PlaneSize")] // Put in add component menu in Unity editor
public class planeSize : MonoBehaviour
{
    public Vector3 COLLIDER_SIZE;
    public Vector3 RENDERER_SIZE;
    // Start is called before the first frame update
    void Start()
    {
        COLLIDER_SIZE = GetComponent<Collider>().bounds.size;
        RENDERER_SIZE = GetComponent<Renderer>().bounds.size;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
