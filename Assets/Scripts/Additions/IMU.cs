using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IMU : MonoBehaviour
{
    float updateFreq = 2;
    public Vector3 angVel;
    Vector3 angAccel;
    public Vector3 linVel;
    Vector3 linAccel;
    private Vector3 lastPos;
    private Vector3 lastAng;
    private Vector3 lastLinVel;
    private Vector3 lastAngVel;
    private float timer = 0;
    public Text IMUData;

    void FixedUpdate()
    {
        timer += Time.deltaTime;
        if (timer > (1 / updateFreq))
        {

            lastLinVel = linVel;
            lastAngVel = angVel;

            Vector3 lastPosInv = transform.InverseTransformPoint(lastPos);

            linVel.x = (0 - lastPosInv.x) / timer;
            linVel.y = (0 - lastPosInv.y) / timer;
            linVel.z = (0 - lastPosInv.z) / timer;

            float deltaX = Mathf.Abs((transform.rotation.eulerAngles).x) - lastAng.x;
            if (Mathf.Abs(deltaX) < 180 && deltaX > -180) angVel.x = deltaX / timer;
            else
            {
                if (deltaX > 180) angVel.x = (360 - deltaX) / timer;
                else angVel.x = (360 + deltaX) / timer;
            }

            float deltaY = Mathf.Abs((transform.rotation.eulerAngles).y) - lastAng.y;
            if (Mathf.Abs(deltaY) < 180 && deltaY > -180) angVel.y = deltaY / timer;
            else
            {
                if (deltaY > 180) angVel.y = (360 - deltaY) / timer;
                else angVel.y = (360 - deltaY) / timer;
            }

            float deltaZ = Mathf.Abs((transform.rotation.eulerAngles).z) - lastAng.z;
            if (Mathf.Abs(deltaZ) < 180 && deltaZ > -180) angVel.z = deltaZ / timer;
            else
            {
                if (deltaZ > 180) angVel.z = (360 - deltaZ) / timer;
                else angVel.z = (360 + deltaZ) / timer;
            }


            linAccel.x = (linVel.x - lastLinVel.x) / timer;
            linAccel.y = (linVel.y - lastLinVel.y) / timer;
            linAccel.z = (linVel.z - lastLinVel.z) / timer;
            angAccel.x = ((angVel.x - lastAngVel.x) / timer) / 2;
            angAccel.y = ((angVel.y - lastAngVel.y) / timer) / 2;//2 is grav on moon (1.6)
            angAccel.z = ((angVel.z - lastAngVel.z) / timer) / 2;

            lastPos = transform.position;

            lastAng.x = Mathf.Abs((transform.rotation.eulerAngles).x);
            lastAng.y = Mathf.Abs((transform.rotation.eulerAngles).y);
            lastAng.z = Mathf.Abs((transform.rotation.eulerAngles).z);

            timer = 0;
            IMUData.text = display();
        }

    }

    string display()
    {
        return "linVel: " + linVel + ",\nlinAcc: " + linAccel + ",\nangVel: " + angVel + ",\nangAcc: " + angAccel;
    }

}
