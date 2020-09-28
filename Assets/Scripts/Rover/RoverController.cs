using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RoverController : MonoBehaviour
{
    //public List<WheelPair> axleInfos; // the information about each individual axle

    public GameObject wheelPrefab;
    public Transform wheelsGO;
    public List<Wheel> wheels;
    public Dictionary<string, Wheel> wheelsMap;

    public float speed = 1;

    Vector2 m_move;

    public void Start() {

    }

    public void FixedUpdate() {
        //float motor = maxMotorTorque * Input.GetAxis("Vertical");
        //float steering = maxSteeringAngle * Input.GetAxis("Horizontal");

        Debug.Log(m_move.x);
        m_move *= speed;
        wheels[0].collider.motorTorque = m_move.x * 20;
        wheels[1].collider.motorTorque = m_move.x * 20;
        wheels[2].collider.motorTorque = -m_move.x * 20;
        wheels[3].collider.motorTorque = -m_move.x * 20;
        foreach (Wheel wheel in wheels) {
            if (m_move.y != 0)
                wheel.collider.motorTorque = m_move.y * 20;
            wheel.UpdateVisualMesh();
        }
        //foreach (WheelPair pair in axleInfos) {
        //    if (pair.motor) {
        //        pair.frontWheel.motorTorque = m_motor;
        //        pair.backWheel.motorTorque = m_motor;
        //    }
        //}
    }

    public void OnSteer(InputAction.CallbackContext context) {
        m_move = context.ReadValue<Vector2>();
    }


    public void GenerateWheels() {
        if (wheelsGO == null) {
            wheelsGO = Instantiate(new GameObject("Wheels")).transform;
        }
        while (wheelsGO.transform.childCount > 0) {
            DestroyImmediate(wheelsGO.transform.GetChild(0).gameObject);
        }
        wheels.Clear();
        Vector3 wheelsOrigin = new Vector3(0, 0, -1);
        Vector3 wheelOffset = new Vector3(110, -30, 131);
        string[] wheelNames = { "FL", "RL", "FR", "RR" };
        foreach( string name in wheelNames) {
            GameObject wheel = Instantiate(wheelPrefab, wheelsGO.transform);
            wheel.name = name;
            Vector3 relativeLoc = Vector3.zero;
            relativeLoc.z = name[0] == 'F' ? 1 : -1;
            relativeLoc.x = name[1] == 'L' ? -1 : 1;
            relativeLoc = Vector3.Scale(relativeLoc, wheelOffset);
            relativeLoc.y = wheelOffset.y;
            wheel.transform.localPosition = wheelsOrigin + relativeLoc;
            if (name[1] == 'R') {
                wheel.transform.GetChild(0).GetChild(0).Rotate(0, 180, 0);
            }
            wheels.Add(new Wheel(wheel.GetComponent<WheelCollider>(), wheel.transform.GetChild(0)));
        }

    }

}

[System.Serializable]
public class Wheel {
    public WheelCollider collider;
    public Transform visualMesh;
    public Wheel(WheelCollider collider, Transform visualMesh) {
        this.collider = collider;
        this.visualMesh = visualMesh;
    }

    public void UpdateVisualMesh() {
        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);
        visualMesh.position = position;
        visualMesh.rotation = rotation;
    }
}