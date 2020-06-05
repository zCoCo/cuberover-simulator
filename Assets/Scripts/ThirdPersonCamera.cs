/*
 * Controls a Third Person View Camera around the Rover
 *
 * Author: Oskar Schlueb (NA)
 * Last Update: 08/2019, Schlueb (NA)
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Iris/Rover/Camera/ThirdPersonCamera")] // Put in add component menu in Unity editor
public class ThirdPersonCamera : MonoBehaviour
{

    [Header("Camera")]
    public Transform lookAt;
    public Transform camTransform;
    
    [Header("Settings")]
    public float minYAngle = 5.0f;
    public float maxYAngle = 50.0f;
    public float sensitivityX = 4.0f;
    public float sensitivityY = 1.0f;

    private Camera cam;

    private float distance = 2.0f;
    private float currentX = 0.0f;
    private float currentY = 0.0f;

    private void Start()
    {
        camTransform = transform;
        cam = Camera.main;
    }

    private void Update()
    {
        currentX += Input.GetAxis("Mouse X");
        currentY += Input.GetAxis("Mouse Y");

        currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);
    }

    private void LateUpdate()
    {
        Vector3 dir = new Vector3(0, 0, -distance);
        Quaternion rotation = Quaternion.Euler(sensitivityY*currentY, sensitivityX*currentX, 0);
        camTransform.position = lookAt.position + rotation * dir;
        camTransform.LookAt(lookAt.position);
    }

}
