﻿/*
 * Connects the Rover to the Pseudo-Backend via a CSV log file (kind of like how
 * the DB connects the frontend to the backend)
 *
 * Author: Connor W. Colombo (CMU)
 * Last Update: 6/3/2020, Colombo (CMU)
 */

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Iris/Rover/Communication/BackendConnection")] // Put in add component menu in Unity editor
public class BackendConnection : MonoBehaviour
{
    public string commands_csv_location = "commands";

    [Tooltip("Round-trip Transmission Signal Delay [s]")]
    public short _transmissionDelay = 8;
    public GameObject transmission_slider;

    private short TransmissionDelay
    {
        get => _transmissionDelay;
        set
        {
            _transmissionDelay = value;
            transmission_slider.GetComponent<Slider>().value = value;
        }
    }

    // Called when delay slider is changed
    public void UpdateDelay(float v)
    {
        _transmissionDelay = (short)v;
    }

    [HideInInspector]
    public string CURR_NAME = "LandingSite";
    [HideInInspector]
    public int CURR_COMMLID = 0;

    private TankMovement movement;
    private CameraRecorder recorder;

    private int last_commandHistory_length = 0; // length of command history at last update

    private bool _connect_to_db;
    public bool ConnectToDB {
        private get => _connect_to_db;
        set {
            _connect_to_db = value;
            if (_connect_to_db)
            {
                Connect();
            }
        }
    }

    private void Awake()
    {
        Application.runInBackground = true; // Allow commands to be received and processed even if not in focus
    }

    private void Start()
    {
        movement = GetComponent<TankMovement>();
        recorder = GetComponent<CameraRecorder>();

        Connect();

        // Start Motion Coroutine:
        StartCoroutine(ReceiveCommand());
    }

    // Setup Data Objects to Setup DB Connection
    private void Connect()
    {
        // Initialize Command History Length:
        List<Dictionary<string, object>> data = CSVReader.Read(commands_csv_location);
        last_commandHistory_length = data.Count + 1;
    }

    // Returns to initial (just landed) state:
    public void Reinit()
    {
        CURR_NAME = "LandingSite";
        // CURR_COMMLID = 0; // Don't reset the counter
    }

    private IEnumerator ReceiveCommand()
    {
        while (true)
        {
            // Update Comms:
            if (movement.was_moving && ConnectToDB)
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
                    yield return new WaitForSecondsRealtime(TransmissionDelay); // Do the whole delay up front since it's simpler and doesn't affect the operator's experience.

                    CURR_COMMLID = Convert.ToInt32(data[i]["lookupID"]);
                    CURR_NAME = Convert.ToString(data[i]["name"]);

                    switch (Convert.ToString(data[i]["name"])){
                        case "ARTEMIS_Deploy":
                            movement.Deploy();
                        break;
                        case "ARTEMIS_Undeploy":
                            movement.UnDeploy();
                        break;
                        case "ARTEMIS_SetSignalDelay":
                            TransmissionDelay = (short)Convert.ToDouble(data[i]["amount"]);
                        break;
                        case "TakeScienceImage":
                            recorder.SCREEN_CAP = true; // trigger screen capture
                            break;
                        case "NoOp":
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

                            movement.t_elapsed = 0F; // restart move timer

                            Debug.Log(string.Format("Moving ({0}) in {1} for {2} at {3}", movement.isTurn, dir, movement.distance, movement.m_Speed));
                        break;
                    }

                    last_commandHistory_length = i + 2;
                    // break;
                }
            }
            yield return new WaitForSecondsRealtime(0.5f); // Check for new commands at 2Hz
        }
    }

}
