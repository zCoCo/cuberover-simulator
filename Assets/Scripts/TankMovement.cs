/*
 * Controls and Logs the Rover's Movements using Tank-like Skid Steer
 *
 * Author: Oskar Schlueb (NA)
 * Last Update: 6/3/2020, Colombo (CMU)
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Iris/Rover/TankMovement")] // Put in add component menu in Unity editor
public class TankMovement : MonoBehaviour
{
    [Header("Mechanical Properties")]
    [Tooltip("Used for forward odometry.")]
    public GameObject wheel_sample;
    [Tooltip("Functional Radius of Wheel [cm] (if null, uses bounding box of wheel)")]
    public float? wheel_radius;

    [Header("Slippage Simulation")]
    [Range(0, 1), Tooltip("Ratio of (Distance from Wheel Rotations - Distance Travelled)/Distance Travelled. Note: Distance travelled is measured along only the intended axis of motion.")]
    public float longitudinal_slip_ratio;
    [Tooltip("Meters to right per meter forward")]
    public float lateral_deviation_factor;
    [Tooltip("Standard Deviation [deg] when attempting a turn (achieved angle distribution with commanded angle as mean)")]
    public float turning_std;

    [Header("Animation")]
    [Tooltip("Used for animating wheels while driving.")]
    public MonoBehaviour sensors;
    public GameObject Lwheel_front;
    public GameObject Rwheel_front;
    public GameObject Lwheel_rear;
    public GameObject Rwheel_rear;

    [HideInInspector]
    public bool was_moving = true;

    [HideInInspector]
    public float m_Speed = 1f;                 // How fast the rover moves forward and back. Remains constant.
    [HideInInspector]
    public float m_TurnSpeed = 30f;            // How fast the rover turns in degrees per second.
                                               // Audio to play when the tank is moving.
    [HideInInspector]
    public float distance; //in cm
    [HideInInspector]
    public float turn; // degree
    [HideInInspector]
    public bool isTurn = false; // whether the current move is a turn
    [HideInInspector]
    public float moveDirection = 1F; // >0 for forward
    [HideInInspector]
    public float turnDirection = 1F; // >0 for CW
    [HideInInspector]
    public float t_elapsed = 0f; // time elapsed during the current move

    [HideInInspector]
    public Vector4 start_position; // origin position on the surface after deployment (x,y,z,θ)
    [HideInInspector]
    public Matrix4x4? world_origin; // homog. transform of origin position on the surface after deployment (defaults to identity transform)
    [HideInInspector]
    public Vector3 predeploy_position; // position of the rover before deployment
    [HideInInspector]
    public Quaternion predeploy_rotation; // rotation of the rover before deployment

    // Total Absolute Path Length Traversed for x=linear moves and
    // y=angular moves. Note: Fwd 1m then Back 1m gives a PathLength of 2m.
    [HideInInspector]
    public Vector2 path_length; // [cm,deg]


    private string m_MovementAxisName;          // The name of the input axis for moving forward and back.
    private string m_TurnAxisName;              // The name of the input axis for turning.
    private Rigidbody m_Rigidbody;              // Reference used to move the rover.
    private float m_MovementStickValue;         // The current value of the movement stick control (WASD override)
    private float m_TurnStickValue;             // The current value of the turn input (WASD override)

    private CameraRecorder recorder;
    private BackendConnection backend;
    private Freezer freezer;

    public bool Deployed { get; private set; }

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        recorder = GetComponent<CameraRecorder>();
        backend = GetComponent<BackendConnection>();
        freezer = GetComponent<Freezer>();

        freezer.freeze = true; // freeze in space (undeployed)

        // Set up the axes
        m_MovementAxisName = "Vertical";
        m_TurnAxisName = "Horizontal";
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

    // Trigger Deploy by a Button Press (rather than command from backend):
    public void DeployByButton()
    {
        backend.CURR_NAME = "Deploy";
        Deploy();
    }

    // Deploy Rover from Peregrine
    public void Deploy()
    {
        if (!Deployed)
        {
            // Capture Predeployment state:
            predeploy_position = m_Rigidbody.position;
            predeploy_rotation = m_Rigidbody.rotation;
            // Deploy:
            freezer.freeze = false; // Unfreeze (disable constraints)
            Deployed = true;
            // Perform Post-Deployment Operations:
            StartCoroutine(AfterDeploy());
        }
    }









    // After the Rover has been Successfully Deployed and Landed:
    IEnumerator AfterDeploy()
    {
        yield return new WaitForSeconds(4); // TODO: Perform an accelerometer (IMU) check
        // Log Deployment Location:
        world_origin = m_Rigidbody.transform.worldToLocalMatrix;
        start_position = GetLocalizationPosition();
        // Capture Image of Deployment Site:
        recorder.SCREEN_CAP = true;
    }

    // Resets the rover to its predeployment state
    public void UnDeploy()
    {
        // TODO: Just reload the scene instead of all this? <- bc you'll want to preserve lid for commands?

        if (Deployed)
        {
            // Return to Predeployment State:
            m_Rigidbody.transform.position = (predeploy_position);
            m_Rigidbody.MoveRotation(predeploy_rotation);

            // Constrain Rover:
            freezer.freeze = true;
            Deployed = false;

            // Reset Path-Dependent Data / States:
            path_length = new Vector2();
            backend.Reinit();
            recorder.Reinit();
        }
    }

    // Toggle Deployment State (for repeated testing)
    public void ToggleDeploy()
    {
        if (Deployed)
        {
            UnDeploy();
        } else
        {
            Deploy();
        }
    }

    private void Start()
    {
        if(wheel_radius == null)
        {
            Vector3 wheel_bounds = wheel_sample.GetComponent<Renderer>().bounds.size;
            wheel_radius = 0.75f * Mathf.Max(wheel_bounds.x, wheel_bounds.y, wheel_bounds.z)/2 * 10; // *10 to convert mm->cm
        }
    }

    private void Update(){
        // Stores the value of both input axes.
        m_MovementStickValue = Input.GetAxis(m_MovementAxisName);
        m_TurnStickValue = Input.GetAxis(m_TurnAxisName);

        // Deploy by Key:
        if(Input.GetKeyDown(KeyCode.Space))
        {
            ToggleDeploy();
        }
    }

    private void FixedUpdate(){
        // Perform Move:
        t_elapsed += Time.fixedDeltaTime;
        if(!isTurn && t_elapsed <= distance/m_Speed){
          // commanded move is still active
          was_moving = false;
          Move(moveDirection);
        } else if(isTurn && t_elapsed <= turn/m_TurnSpeed){
          // commanded turn is still active
          was_moving = false;
          Turn(turnDirection);
        } else if(Mathf.Abs(m_MovementStickValue) > 0.05 || Mathf.Abs(m_TurnStickValue) > 0.05){
          // override movement is being overridden with WASD stick control
          was_moving = false;
          m_Speed = 50;
          backend.CURR_NAME = "ARTEMIS_CorrectiveManeuver"; 
          Move(m_MovementStickValue/3.0f); // div by 3 semi-arbitrarily for better control authority
          Turn(m_TurnStickValue/3.0f);
        } else if(!was_moving){
          was_moving = true;
          recorder.SCREEN_CAP = true; // trigger screen capture
        }

        // Animate Wheels:
        float wheel_scaling_factor = 2 * sensors.GetComponent<Odometer>().R / wheel_radius.Value / 10.0f;
        Lwheel_front.transform.localRotation *= Quaternion.Euler(wheel_scaling_factor * sensors.GetComponent<Odometer>().w_l1 * Time.fixedDeltaTime, 0.0f, 0.0f);
        Lwheel_rear.transform.localRotation *= Quaternion.Euler(wheel_scaling_factor * sensors.GetComponent<Odometer>().w_l2 * Time.fixedDeltaTime, 0.0f, 0.0f);
        Rwheel_front.transform.localRotation *= Quaternion.Euler(wheel_scaling_factor * sensors.GetComponent<Odometer>().w_r1 * Time.fixedDeltaTime, 0.0f, 0.0f);
        Rwheel_rear.transform.localRotation *= Quaternion.Euler(wheel_scaling_factor * sensors.GetComponent<Odometer>().w_r2 * Time.fixedDeltaTime, 0.0f, 0.0f);
}

    // Perform linear movement update. Forward if direction>0, backwards if <0. direction can take values besides (1,0,-1) for WASD override control.
    private void Move(float direction){
        float mag = m_Speed * Time.fixedDeltaTime / 100; // mult by 10 to convert cm to mm
        // Creates a vector in the direction the rover is facing with a magnitude based on the input, speed and the time between frames.
        Vector3 movement = direction * mag * transform.forward;
        // Simulate Longitudinal Slip:
        movement /= (1 + longitudinal_slip_ratio);
        // Simulate Lateral Slip:
        movement += direction * lateral_deviation_factor * mag * transform.right; // still mult by direction since is to absolute left (when driving backwards).

        // Update Path Length (Odometers) (w/out slip):
        path_length[0] += Mathf.Abs(mag*1e2f);

        // Applies this movement to the rigidbody's position.
        m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
    }

    // Perform rotation update. CCW if direction>0, CW if <0. direction can take values besides (1,0,-1) for WASD override control.
    private void Turn(float direction){
        // Determines the number of degrees to be turned based on the input, speed and time between frames.
        float mag = direction * m_TurnSpeed * Time.fixedDeltaTime;

        // Update Path Length (Odometers) (w/out slip):
        path_length[1] += Mathf.Abs(mag);

        // Simulate Turning Inaccuracies:
        mag += Util.NormRand(0, turning_std, 5);

        // Makes this into a rotation in the y axis.
        Quaternion turnRotation = Quaternion.Euler(0f, mag, 0f);

        // Applies this rotation to the rigidbody's rotation.
        m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);
    }

    // Returns the Rover's Displacement (3D Translational and heading =
    // (x,y,z,θ)) since deployment in [cm,cm,cm,deg]:
    public Vector4 GetDisplacement()
    {
        return (GetLocalizationPosition() - start_position);
    }

    // Returns the Absolute (arbitrary world frame reference) (x,y,z,θ) [cm,cm,cm,deg]: Coordinates of the Rover (translational and heading)
    private Vector4 GetLocalizationPosition()
    {
        float deg = Mathf.PI / 180f; // deg->rad
        float mm = 1e-1f; // mm -> cm

        Vector3 rPw;
        if (world_origin != null)
        {
            rPw = world_origin.GetValueOrDefault().MultiplyPoint3x4(m_Rigidbody.position); // Rover position w.r.t. world frame on surface (world_origin)
        }
        else
        {
            rPw = m_Rigidbody.position;
        }
        
        return new Vector4(rPw.x*mm, rPw.y*mm, rPw.z*mm, Mathf.Atan2(Mathf.Sin(m_Rigidbody.rotation.eulerAngles.y*deg), Mathf.Cos(m_Rigidbody.rotation.eulerAngles.y*deg))/deg);
    }
}
