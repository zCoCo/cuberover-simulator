using UnityEngine;
using System.Collections;

[AddComponentMenu("Iris/Rover/Sensors/Odometer")] // Put in add component menu in Unity editor
public class Odometer : MonoBehaviour
{

    private float V = 0; // x velocity (forward+)
    private float omega; //rotational speed CCW+
    private float V_x;
    private float V_y;
    Vector3 lv;
    Vector3 av;

    private float v_r; //= w_r * R
    private float v_l;
    private float w_r;//= v_r / R
    private float w_l;
    public float w_l1;
    public float w_l2;
    public float w_r1;
    public float w_r2;
    public float scale_l1 = 1;
    public float scale_l2 = 1;
    public float scale_r1 = 1;
    public float scale_r2 = 1;


    //declared in editor:
    public float R; //radius of wheels
    public float d; //
    //slip ratios
    private float beta = 0;//angle
    public float alpha_r = 0;
    public float alpha_l = 0;

    public GameObject rover;
    private TankMovement script;

    // Use this for initialization
    void Start()
    {
        script = rover.GetComponent<TankMovement>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(script.Deployed){
            // Perform actual 4-wheel skid-steer calculations to determine what odometers would actually read
            // use this to reconstruct x,y,theta if necessary
            lv = gameObject.GetComponent<IMU>().linVel;
            av = gameObject.GetComponent<IMU>().angVel;
            V_x = lv.z;
            V_y = lv.y;
            V = Mathf.Sqrt(Mathf.Pow(lv.x, 2) + Mathf.Pow(lv.z, 2) + Mathf.Pow(lv.y, 2)); //gets net velocity
            omega = av.y * -1 * Mathf.PI / 180;

            //(d*omega-(sqrt(2)Vsqrt(cos(2B)+1))/2)/R*(alpha_l-1)
            w_l = (d * omega - (Mathf.Sqrt(2) * V * Mathf.Sqrt(Mathf.Cos(2 * beta) + 1)) / 2) / (R * (alpha_l - 1)) * 180 / Mathf.PI;
            w_l1 = w_l * scale_l1 + NormRand(0, (float)0.5, 5);
            w_l2 = w_l * scale_l2 + NormRand(0, (float)0.5, 5);
            //round to hundreths place
            w_l1 = Mathf.Round(w_l1 * 100.0f) * 0.01f;
            w_l2 = Mathf.Round(w_l2 * 100.0f) * 0.01f;

            //-(2*omega + sqrt(2)*V * sqrt(cos(2 * beta) + 1))/(2R * (alpha_r - 1))
            w_r = -1 * (2 * omega * d + Mathf.Sqrt(2) * V * Mathf.Sqrt(Mathf.Cos(2 * beta) + 1)) / (2 * R * (alpha_r - 1)) * 180 / Mathf.PI;
            w_r1 = w_r * scale_r1 + NormRand(0, (float)0.5, 5);
            w_r2 = w_r * scale_r2 + NormRand(0, (float)0.5, 5);
            //round to hundreths place
            w_r1 = Mathf.Round(w_r1 * 100.0f) * 0.01f;
            w_r2 = Mathf.Round(w_r2 * 100.0f) * 0.01f;

        }
        else
        {
            w_l1 = 0;
            w_l2 = 0;
            w_r1 = 0;
            w_r2 = 0;
        }

    }

    public static float NormRand(float mu = 0, float sig = 1, float? z_max = null)
    {
        float u, v, S;

        do
        {
            u = 2.0f * Random.value - 1.0f;
            v = 2.0f * Random.value - 1.0f;
            S = u * u + v * v;
        }
        while (S >= 1.0f);

        float z = u * Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);

        if (z_max != null && Mathf.Abs(z) > z_max)
        {
            z = (float)z_max;
        }

        return z * sig + mu;
    }

    private void Reset()
    {
        // TODO: This might be necessary.
    }
}
