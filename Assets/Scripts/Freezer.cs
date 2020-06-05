// Freezes a GameObject in Space
using UnityEngine;

[AddComponentMenu("Iris/Util/Freezer")] // Put in add component menu in Unity editor
public class Freezer : MonoBehaviour
{
    public bool freeze; // public bool allows for toggling in Inspector
    private bool prev_freeze; // freeze state on previous check

    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (freeze && !prev_freeze)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
        else if (!freeze && prev_freeze)
        {
            rb.constraints = RigidbodyConstraints.None;
        }
        prev_freeze = freeze;
    }
}
