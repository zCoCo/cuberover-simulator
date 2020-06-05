/*
 * Sets the Absolute Size of the Given Component to Given Dimensions in cm.
 *
 * Author: Connor W. Colombo (CMU)
 * Last Update: 6/4/2020, Colombo (CMU)
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways] // Run in editor too (and support prefabs)
[AddComponentMenu("Iris/Util/SetAbsoluteSize")] // Put in add component menu in Unity editor
public class SetAbsoluteSize : MonoBehaviour
{
    public Vector3 size_in_cm;
    [Tooltip("Number of cm per game unit.")]
    public float scale = 0.01f;

    private Vector3 new_scale;
    private Renderer rdr;
    private Transform tf;

    void Start() { 
        Resize();
    }

#if UNITY_EDITOR
    // Called when changes are made in the editor (so they can be seen while editing)
    void OnValidate()
    {
        Resize();
    }
#endif

    void Resize()
    {
        rdr = GetComponent<Renderer>();
        tf = GetComponent<Transform>();

        if (!(Mathf.Approximately(rdr.bounds.size.x, 0f) || Mathf.Approximately(rdr.bounds.size.y, 0f) || Mathf.Approximately(rdr.bounds.size.z, 0f)))
        {
            tf.localScale = (Mathf.Abs(tf.localScale.x) < scale*1e-3f || Mathf.Abs(tf.localScale.y) < scale*1e-3f || Mathf.Abs(tf.localScale.z) < scale*1e3f) ? new Vector3(scale,scale,scale) : tf.localScale;

            new_scale.x = size_in_cm.x * scale / (rdr.bounds.size.x / tf.localScale.x);
            new_scale.y = size_in_cm.y * scale / (rdr.bounds.size.y / tf.localScale.y);
            new_scale.z = size_in_cm.z * scale / (rdr.bounds.size.z / tf.localScale.z);

            // Ensure no scales are approx 0 (never can leave 0):
            new_scale.x = Mathf.Approximately(new_scale.x, 0f) ? tf.localScale.x : new_scale.x;
            new_scale.y = Mathf.Approximately(new_scale.y, 0f) ? tf.localScale.y : new_scale.y;
            new_scale.z = Mathf.Approximately(new_scale.z, 0f) ? tf.localScale.z : new_scale.y;

            tf.localScale = new_scale;
        }
    }
}
