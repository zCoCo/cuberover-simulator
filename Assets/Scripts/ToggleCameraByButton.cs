/*
 * Allows the Connected Camera to be Toggled Active/Inactive by a GUI Button.
 *
 * Author: Connor W. Colombo (CMU)
 * Last Update: 5/31/2020, Colombo (CMU)
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Iris/Interface/ToggleCameraByButton")] // Put in add component menu in Unity editor
public class ToggleCameraByButton : MonoBehaviour
{

    public Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    public void OnButtonChange(bool new_state)
    {
        cam.gameObject.SetActive(new_state);
    }
}
