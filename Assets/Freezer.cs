// Freezes a GameObject in Space
using UnityEngine;

public class Freezer : MonoBehaviour
{
    public bool freeze;
    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (freeze)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
