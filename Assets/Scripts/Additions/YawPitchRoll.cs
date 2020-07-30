using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class YawPitchRoll : MonoBehaviour
{
    public int PitchDeg;
    public int YawDeg;
    public int RollDeg;
    public float speed;
    public Text orientation;
    public Vector3 lastPosition;
    public Vector3 currentPosition;
    public Vector3 velocity;

    // Start is called before the first frame update
    void Start()
    {
        lastPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        currentPosition = transform.position / 10;
        velocity = (currentPosition - lastPosition);
        speed = Vector3.Distance(lastPosition, currentPosition);
        PitchDeg = 360 - (int)this.transform.rotation.eulerAngles.x;
        YawDeg = (int)this.transform.rotation.eulerAngles.y;
        RollDeg = 360 - (int)this.transform.rotation.eulerAngles.z;

        orientation.text = display();

    }
    int getDegree(string d)
    {
        if (d.Equals("Pitch"))
            return PitchDeg;
        if (d.Equals("Yaw"))
            return YawDeg;
        if (d.Equals("Roll"))
            return RollDeg;
        else
            return -1;
    }

    string display()
    {
        return "Pitch: " + PitchDeg.ToString() + "deg\nYaw: " + YawDeg.ToString() + "deg\nRoll: " + RollDeg.ToString() + "deg"; ;
    }
}
