using UnityEngine;
using System.Collections;

/// <summary>
/// Mimicks the results expected if the rover were to have an onboard
/// localization system.
/// </summary>
[AddComponentMenu("Iris/Rover/Sensors/PseudoLocalizer")] // Put in add component menu in Unity editor
public class PseudoLocalizer : PseudoSensor
{

    public override dynamic GetSensorData()
    {
        return new {
            a = 5,
            b = "Hello",
            c = new[] { 4, 5, 6 }
        };
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Perform actual 4-wheel skid-steer calculations to determine what odometers would actually read
        // use this to reconstruct x,y,theta if necessary
    }

    private void Reset()
    {
        // TODO: This might be necessary.
    }
}
