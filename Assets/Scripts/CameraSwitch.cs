using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    Character_Input characterInput;
    Rigidbody mybody;

    //public float speed;

    public Camera fpsCam;
    public Camera tpsCam;
    public GameObject Lwheel;
    public GameObject Rwheel;

    bool fpsView = true;
    bool locked = false;


    // Start is called before the first frame update
    void Start()
    {
        characterInput = GetComponent<Character_Input>();
        mybody = GetComponent<Rigidbody>();

        //Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        //float horizontal = Input.GetAxis("Horizontal");
        //float vertical = Input.GetAxis("Vertical");

        //Vector3 moveDirection = new Vector3(0f, 0f, vertical) * speed * Time.deltaTime;
        //transform.Translate(moveDirection);

        if (Input.GetKeyDown(characterInput.toggleCam))
        {
            ToggleCamera();
            
        }
        if (locked == true)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Lwheel.SetActive(true);
            Rwheel.SetActive(true);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Lwheel.SetActive(false);
            Rwheel.SetActive(false);
        }

    }



    void ToggleCamera()
    {
        fpsView = !fpsView;
        fpsCam.gameObject.SetActive(fpsView);
        tpsCam.gameObject.SetActive(!fpsView);
        locked = !locked;
        
    }
}