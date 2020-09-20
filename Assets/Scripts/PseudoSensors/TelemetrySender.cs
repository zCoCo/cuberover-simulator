using UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Malee.List;

/// <summary>
/// Class for sending data to the backend at a fixed datarate from a
/// BufferedSensorReaders.
///
/// Author: Connor Colombo (CMU)
/// Last Update: 7/6/2020, Colombo (CMU)
/// </summary>
[AddComponentMenu("Iris/Rover/Communication/TelemetrySender")] // Put in add component menu in Unity editor
public class TelemetrySender : MonoBehaviour
{
    [System.Serializable]
    public class SensorList : ReorderableArray<BufferedSensorReader> { };
    [Reorderable(null, "Sensor", null)]
    public SensorList sensors;

    public string socket = "5555";
    public string collection = "None"; // Collection in DB to push to

    private void Start()
    {
        StartCoroutine(ReadSensors());
    }

    /// <summary>
    /// Read Data from Each of the Sensors as Necessary.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ReadSensors()
    {
        foreach(BufferedSensorReader sensor in sensors)
        {
            float now = Time.realtimeSinceStartup; // capture time once (for consistent comparisons)
            if (now - sensor.time_last_reading > 1 / sensor.poll_rate)
            {
                //sensor.buffer
                //sensor.time_last_reading = now;
            }
        }

        float delay = 1 / sensors.Max(x => x.poll_rate) / 2; // Call at least twice per polling cycle for every sensor.
        yield return new WaitForSecondsRealtime(delay > 1 ? 1 : delay); // Call at least once per second.
    }

    /*
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    public Send(T message)
    {
        buffer.Enqueue(message);
    }


    private void Start()
    {

        telem.Send(new Dictionary<string, float>()
        {
            {"accelX", 1f},
            {"accelY", 1f},
            {"accelZ", 1f}
        });

    }

    private void OnDestroy()
    {

    }*/
}