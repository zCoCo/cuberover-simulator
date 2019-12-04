using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    public static bool USE_FRONT_CAM = true;

    Character_Input characterInput;
    Rigidbody mybody;

    public Camera frontCam;
    public Camera rearCam;
    public GameObject Lwheel_front;
    public GameObject Rwheel_front;
    public GameObject Lwheel_rear;
    public GameObject Rwheel_rear;

    bool frontView = true;
    bool locked = false;

    // Start is called before the first frame update
    void Start(){
        characterInput = GetComponent<Character_Input>();
        mybody = GetComponent<Rigidbody>();
        UpdateCameraView(); // Initialize (make sure only one camera is active and wheels are hidden)
    }

    // Update is called once per frame
    void Update(){
        if (Input.GetKeyDown(characterInput.toggleCam)){
            ToggleCamera();
        }
        if(CameraSwitch.USE_FRONT_CAM != frontView){ // allow for external manipulation
          frontView = CameraSwitch.USE_FRONT_CAM;
          UpdateCameraView();
          if(!CameraSwitch.USE_FRONT_CAM){
            ScreenRecorder.SCREEN_CAP = true; // take second pic
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
      Lwheel_front.SetActive(!frontView);
      Rwheel_front.SetActive(!frontView);
      Lwheel_rear.SetActive(frontView);
      Rwheel_rear.SetActive(frontView);
    }
}
