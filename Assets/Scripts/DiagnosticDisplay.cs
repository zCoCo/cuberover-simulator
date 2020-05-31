﻿/*
 * Displays a Diagnostics Panel in the GUI Canvas.
 *
 * Author: Connor W. Colombo (CMU)
 * Last Update: 5/31/2020, Colombo (CMU)
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiagnosticDisplay : MonoBehaviour
{
    public GameObject rover;

    private BackendConnection backend;
    private TankMovement movement;
    
    // Start is called before the first frame update
    void Start()
    {
        backend = rover.GetComponent<BackendConnection>();
        movement = rover.GetComponent<TankMovement>();
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(20, 20, 400, 120));
        GUILayout.Label(string.Format("Command {0}: {1}({2},{3})", backend.CURR_COMMLID, backend.CURR_NAME, (movement.isTurn ? movement.turnDirection * movement.turn : movement.moveDirection * movement.distance), (movement.isTurn ? movement.m_TurnSpeed : movement.m_Speed)));
        GUILayout.Label("----");
        GUILayout.Label(string.Format("True Location (x,y,z,θ): {0}", movement.GetDisplacement().ToString("F3")));
        GUILayout.Label(string.Format("Odometry: {0}m, {1}deg", movement.path_length[0], movement.path_length[1]));
        GUILayout.EndArea();
    }
}
