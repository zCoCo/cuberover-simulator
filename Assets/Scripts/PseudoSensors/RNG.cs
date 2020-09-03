using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RNG : MonoBehaviour
{

    public float input;
    public float output;
    public double bounds = 0.5;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        output = input + NormRand(0, (float)bounds, 5);
        output = Mathf.Round(output * 100.0f) * 0.01f;
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

    float getOutput()
    {
        return output;
    }
}
