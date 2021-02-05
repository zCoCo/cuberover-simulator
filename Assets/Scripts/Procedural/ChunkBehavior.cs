using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Voxel;

public class ChunkBehavior : MonoBehaviour
{
    public Chunk chunk;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDrawGizmos() {
        if (chunk != null && chunk.data != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(chunk.data[0].xyz, chunk.data[chunk.data.Length - 1].xyz);
        }
    }
}
