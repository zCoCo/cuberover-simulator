using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetAbsoluteSize : MonoBehaviour
{
    public Vector3 size_in_cm;
    private Vector3 new_scale;
    private Renderer rdr;

    // Start is called before the first frame update
    void Start()
    {
        rdr = GetComponent<Renderer>();

        new_scale.x = size_in_cm.x * 0.01f / (rdr.bounds.size.x / GetComponent<Transform>().localScale.x);
        new_scale.y = size_in_cm.y * 0.01f / (rdr.bounds.size.y / GetComponent<Transform>().localScale.y);
        new_scale.z = size_in_cm.z * 0.01f / (rdr.bounds.size.z / GetComponent<Transform>().localScale.z);

        GetComponent<Transform>().localScale = new_scale;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
