/*
 * Connects the Rover to the Pseudo-Backend via a CSV log file (kind of like how
 * the DB connects the frontend to the backend)
 *
 * Author: Connor W. Colombo (CMU)
 * Last Update: 5/31/2020, Colombo (CMU)
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class BackendConnection : MonoBehaviour
{
    public string commands_csv_location = "commands";

    [Tooltip("Round-trip Transmission Signal Delay [s]")]
    public float signal_delay = 8;

    [HideInInspector]
    public string CURR_NAME = "LandingSite";
    [HideInInspector]
    public int CURR_COMMLID = 0;

    private TankMovement movement;

    private int last_commandHistory_length = 0; // length of command history at last update

    private void Awake()
    {
        Application.runInBackground = true; // Allow commands to be received and processed even if not in focus
    }

    private void Start()
    {
        movement = GetComponent<TankMovement>();

        // Initialize Command History Length:
        List<Dictionary<string, object>> data = CSVReader.Read(commands_csv_location);
        last_commandHistory_length = data.Count + 1;

        // Start Motion Coroutine:
        StartCoroutine(ReceiveCommand());
    }

    // Returns to initial (just landed) state:
    public void Reset()
    {
        CURR_NAME = "LandingSite";
        CURR_COMMLID = 0;
    }

    private IEnumerator ReceiveCommand()
    {
        while (true)
        {
            // Update Comms:
            if (movement.was_moving)
            {
                List<Dictionary<string, object>> data = CSVReader.Read(commands_csv_location);
                var i0 = last_commandHistory_length - 1;
                if (i0 < 0)
                {
                    i0 = 0;
                }
                int i = i0;
                if (i < data.Count)
                {

                    CURR_COMMLID = Convert.ToInt32(data[i]["lookupID"]);
                    CURR_NAME = Convert.ToString(data[i]["name"]) + Convert.ToString(CURR_COMMLID);

                    switch (Convert.ToString(data[i]["name"])){
                        case "ARTEMIS_Deploy":
                            movement.Deploy();
                        break;
                        case "ARTEMIS_Undeploy":
                            movement.UnDeploy();
                        break;
                        case "ARTEMIS_SetSignalDelay":
                            signal_delay = (float)Convert.ToDouble(data[i]["delay"]);
                        break;
                        default:
                            Debug.Log("Received amount " + data[i]["amount"] + " " +
                                   "speed " + data[i]["speed"] + " " +
                                   "isTurn " + data[i]["isTurn"]);

                            movement.distance = (float)Convert.ToDouble(data[i]["amount"]);
                            movement.m_Speed = (float)Convert.ToDouble(data[i]["speed"]);
                            movement.turn = (float)Convert.ToDouble(data[i]["amount"]);
                            movement.m_TurnSpeed = (float)Convert.ToDouble(data[i]["speed"]);
                            movement.isTurn = Convert.ToInt32(data[i]["isTurn"]) == 1;
                            var dir = (float)Convert.ToDouble(data[i]["dir"]);
                            movement.moveDirection = dir;
                            movement.turnDirection = dir;

                            yield return new WaitForSecondsRealtime(signal_delay); // Do the whole delay up front since it's simpler and doesn't affect the operator's experience.
                            movement.t_elapsed = 0F; // restart move timer

                            Debug.Log(string.Format("Moving ({0}) in {1} for {2} at {3}", movement.isTurn, dir, movement.distance, movement.m_Speed));

                            last_commandHistory_length = i + 2;
                        break;
                    }
                    // break;
                }
            }
            yield return new WaitForSecondsRealtime(0.5f); // Check for new commands at 2Hz
        }
    }

}
