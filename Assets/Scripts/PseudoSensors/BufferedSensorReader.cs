using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Wraps a PseudoSensor allowing for collection of data from it at a fixed
/// frequency into a FIFO buffer, from which data will be pushed to the backend
/// at a rate determined by this sensor's bandwidth allocation.
/// 
/// Keeping this functionality in a wrapper instead of in a pseudosensor class
/// allows all telemetry related data to be edited in a single place in the
/// Unity Inspector even if the sensors are attached to different GameObjects
/// than the TelemetrySender.
/// 
/// NOTE: The actual timing of data collection and sending is to be implemented
/// externally (in the TelemeterySender).
///
/// NOTE: T represents the data type used to store data internally. The data 
/// will actually be sent to the backend as the type determined by
/// cast_before_send.
///
/// Author: Connor Colombo (CMU)
/// Last Update: 7/6/2020, Colombo (CMU)
/// </summary>
[System.Serializable]
public class BufferedSensorReader
{
    [Header("Sensor Hookup")]

    [Tooltip("PseudoSensor object being read from.")]
    public MonoBehaviour sensor;

    [Tooltip("Type to cast data from the sensor to before sending.")]
    public ValidDataType cast_before_send;

    public enum ValidDataType
    {
        Float,
        UInt8,
        Int8,
        UInt16,
        Int16,
        UInt32,
        Int32
    }

    [Tooltip("[Hz] Rate at which data is to be collected from the sensor.")]
    [Range(0, 1000)]
    public float poll_rate;

    [Header("Backend Hookup")]

    [Tooltip("Target collection in the database where data from the sensor should go.")]
    public string target_collection = "None";

    [Tooltip("[kbps] Maximum bandwidth allocated for this sensor.")]
    [Range(0, 20)]
    public float bandwidth_allocation;


    [HideInInspector]
    public float time_last_reading; // (real) time of the last reading

    /// <summary>
    /// Whether data should be read from this sensor currently.
    /// </summary>
    private bool collecting = true;

    private Queue<Dictionary<string,float>> buffer;

    /// <summary>
    /// Reads the Sensor (saves dictionary value to the queue).
    /// </summary>
    public void Read()
    {
        if (collecting)
        {
            //buffer.Enqueue(sensor.GetSensorDictionary());
        }
    }
}
