// Common Utility Functions

using UnityEngine;

public static class Util
{
    // Returns a random number along the normal distribution defined by the
    // given mean and standard deviation using the Marsaglia polar method.
    // Clips the distribution at +- z_max if given
    public static float NormRand(float mu=0, float sig=1, float? z_max=null)
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

        if(z_max != null && Mathf.Abs(z) > z_max)
        {
            z = (float)z_max;
        }

        return z * sig + mu;
    }
}
