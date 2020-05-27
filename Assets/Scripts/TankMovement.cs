using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TankMovement : MonoBehaviour
{
    public static string CURR_NAME = "LandingSite";
    public static int CURR_COMMLID = 0;

    private bool was_moving = true;

    private float m_Speed = 1f;                 // How fast the rover moves forward and back. Remains constant.
    public float m_TurnSpeed = 30f;            // How fast the rover turns in degrees per second.
                                                // Audio to play when the tank is moving.
    public float distance; //in cm
    public float turn; // degree
    public bool isTurn = false; // whether the current move is a turn
    public float moveDirection = 1F; // >0 for forward
    public float turnDirection = 1F; // >0 for CW
    private float t_elapsed = 0f; // time elapsed during the current move
    private int last_commandHistory_length = 0; // length of command history at last update


    private string m_MovementAxisName;          // The name of the input axis for moving forward and back.
    private string m_TurnAxisName;              // The name of the input axis for turning.
    private Rigidbody m_Rigidbody;              // Reference used to move the rover.
    private float m_MovementStickValue;         // The current value of the movement stick control (WASD override)
    private float m_TurnStickValue;             // The current value of the turn input (WASD override)

    /*
     instructions:

     m_speed is constant at 1 but manipulate distance bc dis at 2 goes 2cm/sec and dis of 3 goes 3cm/sec. turnspeed @30 goes 30deg/sec and turn is either -1,0,1 to indicate l/r
     the outside code being written should change the distance, turn, and turnspeed variables. Unity does not have sleep commands that would work best to calculate the time it should run in order to go the correct distance so
     if the code could do that on that end it would work flawlessly. As in when inputted 1 for distance, it would change that variable then wait 1 sec then automatially send 0 to stop the rover. Or else the rovver will continuesly move at a rate of 1cm/sec.

    */

    private void Awake()
    {
        Application.runInBackground = true; // Allow commands to be received and processed even if not in focus
        m_Rigidbody = GetComponent<Rigidbody>();
    }


    private void OnEnable()
    {
        // When the rover is turned on, this makes sure it's not kinematic.
        m_Rigidbody.isKinematic = false;

        // Also reset the input values.
        m_MovementStickValue = 0f;
        m_TurnStickValue = 0f;
    }


    private void OnDisable()
    {
        // When the rover is turned off, this sets it to kinematic so it stops moving.
        m_Rigidbody.isKinematic = true;
    }

    private void Start()
    {
        // Set up the axes
        m_MovementAxisName = "Vertical";
        m_TurnAxisName = "Horizontal";

        // Initialize Command History Length:
        List<Dictionary<string,object>> data = CSVReader.Read("commands");
        last_commandHistory_length = data.Count+1;

        // Start Motion Coroutine:
        StartCoroutine(ReceiveCommand());
    }


    private void Update(){
        // Stores the value of both input axes.
        m_MovementStickValue = Input.GetAxis(m_MovementAxisName); //restore these two lines to regain WASD controls and comment out the following two lines
        m_TurnStickValue = Input.GetAxis(m_TurnAxisName);
    }

    private void FixedUpdate(){
        // Perform Move:
        t_elapsed += Time.deltaTime;
        if(t_elapsed <= distance/m_Speed && !isTurn){
          // commanded move is still active
          was_moving = false;
          Move(moveDirection);
        } else if(t_elapsed <= turn/m_TurnSpeed && isTurn){
          // commanded turn is still active
          was_moving = false;
          Turn(turnDirection);
        } else if(Mathf.Abs(m_MovementStickValue) > 0.05 || Mathf.Abs(m_TurnStickValue) > 0.05){
          // override movement is being overridden with WASD stick control
          was_moving = false;
          m_Speed = 50;
          Move(m_MovementStickValue); // div by 3 semi-arbitrarily for better control authority
          Turn(m_TurnStickValue);
        } else if(!was_moving){
          was_moving = true;
          ScreenRecorder.SCREEN_CAP = true; // trigger screen capture
        }
    }

    private IEnumerator ReceiveCommand() {
        while (true) {
            // Update Comms:
            if(was_moving){
              List<Dictionary<string,object>> data = CSVReader.Read("commands");
              var i0 = last_commandHistory_length-1;
              if(i0<0){
                i0 = 0;
              }
              int i = i0;
              if(i < data.Count){
                Debug.Log("amount " + data[i]["amount"] + " " +
                       "speed " + data[i]["speed"] + " " +
                       "isTurn " + data[i]["isTurn"]);

                distance = (float)Convert.ToDouble(data[i]["amount"]);
                m_Speed = (float)Convert.ToDouble(data[i]["speed"]);
                turn = (float)Convert.ToDouble(data[i]["amount"]);
                m_TurnSpeed = (float)Convert.ToDouble(data[i]["speed"]);
                isTurn = Convert.ToInt32(data[i]["isTurn"]) == 1;
                var dir = (float)Convert.ToDouble(data[i]["dir"]);
                moveDirection = dir;
                turnDirection = dir;
                t_elapsed = 0F; // restart move timer

                CURR_COMMLID = Convert.ToInt32(data[i]["lookupID"]);
                CURR_NAME = Convert.ToString(data[i]["name"]) + Convert.ToString(CURR_COMMLID);

                Debug.Log(string.Format("Moving ({0}) in {1} for {2} at {3}", isTurn, dir, distance,m_Speed));

                last_commandHistory_length = i + 2;
                // break;
              }
            }
            yield return new WaitForSecondsRealtime(0.5f); // Check for new commands at 2Hz
        }
    }

    // Perform linear movement update. Forward if direction>0, backwards if <0. direction can take values besides (1,0,-1) for WASD override control.
    private void Move(float direction){
        // Creates a vector in the direction the rover is facing with a magnitude based on the input, speed and the time between frames.
        Vector3 movement = direction * transform.forward * m_Speed * Time.deltaTime / 100; // mult by 10 to convert cm to mm

        // Applies this movement to the rigidbody's position.
        m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
    }

    // Perform rotation update. CCW if direction>0, CW if <0. direction can take values besides (1,0,-1) for WASD override control.
    private void Turn(float direction){
        // Determines the number of degrees to be turned based on the input, speed and time between frames.
        float turn = direction * m_TurnSpeed * Time.deltaTime;

        // Makes this into a rotation in the y axis.
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);

        // Applies this rotation to the rigidbody's rotation.
        m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);
    }
}
