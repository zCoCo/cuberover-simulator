/*
 * Toggles the Active Camera
 *
 * Author: Oskar Schlueb (NA)
 * Last Update: 5/31/2020, Colombo (CMU)
 *
 * @TODO: Absorb this into CameraRecorder.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Iris/Rover/Camera/CameraSwitch")] // Put in add component menu in Unity editor
public class CameraSwitch : MonoBehaviour
{
    public static bool USE_FRONT_CAM = true;

    Character_Input characterInput;

    public Camera frontCam;
    public Camera rearCam;
    public Camera depthCam;
    //public GameObject Lwheel_front;
    //public GameObject Rwheel_front;
    //public GameObject Lwheel_rear;
    //public GameObject Rwheel_rear;

    bool frontView = true;
    bool locked = false;

    private CameraRecorder recorder;

    // Start is called before the first frame update
    void Start(){
        characterInput = GetComponent<Character_Input>();
        recorder = GetComponent<CameraRecorder>();
        UpdateCameraView(); // Initialize (make sure only one camera is active and appropriate wheels are hidden)
    }

    // Update is called once per frame
    void Update(){
        /*if (Input.GetKeyDown(characterInput.toggleCam)){
            ToggleCamera();
        }*/
        if(CameraSwitch.USE_FRONT_CAM != frontView){ // allow for external manipulation
          frontView = CameraSwitch.USE_FRONT_CAM;
          UpdateCameraView();
          if(!CameraSwitch.USE_FRONT_CAM){
            recorder.SCREEN_CAP = true; // take second pic
          }
        }
        if (locked == true){
            Cursor.lockState = CursorLockMode.Locked;
        } else{
            Cursor.lockState = CursorLockMode.None;
        }
    }

    void ToggleCamera(){
        frontView = !frontView;
        CameraSwitch.USE_FRONT_CAM = frontView;
        //locked = !locked;
        UpdateCameraView();
    }

    void UpdateCameraView(){
      frontCam.gameObject.SetActive(frontView);
      rearCam.gameObject.SetActive(!frontView);
      // Hide Wheels which would be in view:
      //Lwheel_front.SetActive(!frontView);
      //Rwheel_front.SetActive(!frontView);
      //Lwheel_rear.SetActive(frontView);
      //Rwheel_rear.SetActive(frontView);
    }
}
