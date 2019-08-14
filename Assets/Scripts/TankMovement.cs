using UnityEngine;

public class TankMovement : MonoBehaviour
{
    private float m_Speed = 1f;                 // How fast the rover moves forward and back. Remains constant.
    public float m_TurnSpeed = 30f;            // How fast the rover turns in degrees per second.
                                                // Audio to play when the tank is moving.
    public float distance; //in cm per sec
    public float turn; // degree per sec
    

    private string m_MovementAxisName;          // The name of the input axis for moving forward and back.
    private string m_TurnAxisName;              // The name of the input axis for turning.
    private Rigidbody m_Rigidbody;              // Reference used to move the rover.
    private float m_MovementInputValue;         // The current value of the movement input.
    private float m_TurnInputValue;             // The current value of the turn input.

    /*
     instructions:

     m_speed is constant at 1 but manipulate distance bc dis at 2 goes 2cm/sec and dis of 3 goes 3cm/sec. turnspeed @30 goes 30deg/sec and turn is either -1,0,1 to indicate l/r
     the outside code being written should change the distance, turn, and turnspeed variables. Unity does not have sleep commands that would work best to calculate the time it should run in order to go the correct distance so
     if the code could do that on that end it would work flawlessly. As in when inputted 1 for distance, it would change that variable then wait 1 sec then automatially send 0 to stop the rover. Or else the rovver will continuesly move at a rate of 1cm/sec.

    */

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }


    private void OnEnable() 
    {
        // When the rover is turned on, this makes sure it's not kinematic.
        m_Rigidbody.isKinematic = false;

        // Also reset the input values.
        m_MovementInputValue = 0f;
        m_TurnInputValue = 0f;
    }


    private void OnDisable()
    {
        // When the rover is turned off, this sets it to kinematic so it stops moving.
        m_Rigidbody.isKinematic = true;
    }

    private void Start()
    {
        //sets up the axis
        m_MovementAxisName = "Vertical";
        m_TurnAxisName = "Horizontal";

    }


    private void Update()
    {
        // Stores the value of both input axes.
        //m_MovementInputValue = Input.GetAxis(m_MovementAxisName); //restore these two lines to regain WASD controls and comment out the following two lines
        //m_TurnInputValue = Input.GetAxis(m_TurnAxisName);
        m_MovementInputValue = distance/10;
        m_TurnInputValue = turn;//set either pos or neg to go left/right, adjust m_turnspeed for degrees

    }


    private void FixedUpdate()
    {
        // Adjusts the rigidbodies position and orientation in FixedUpdate.
        Move();
        Turn();
    }


    private void Move()
    {
        // Creates a vector in the direction the rover is facing with a magnitude based on the input, speed and the time between frames.
        Vector3 movement = transform.forward * m_MovementInputValue * m_Speed * Time.deltaTime;

        // Applys this movement to the rigidbody's position.
        m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
    }


    private void Turn()
    {
        // Determines the number of degrees to be turned based on the input, speed and time between frames.
        float turn = m_TurnInputValue * m_TurnSpeed * Time.deltaTime;

        // Makes this into a rotation in the y axis.
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);

        // Applies this rotation to the rigidbody's rotation.
        m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);
    }
}