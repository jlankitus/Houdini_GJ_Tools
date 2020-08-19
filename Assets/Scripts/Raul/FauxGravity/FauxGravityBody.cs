using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FauxGravityBody : MonoBehaviour
{
    [Tooltip("The Attractor which should Pull this Object")]
    public FauxGravityAttractor attractor;

    private Transform myTransform;
    private Rigidbody myRigidbody;
    // Start is called before the first frame update
    void Start()
    {
        myTransform = transform;
        myRigidbody = GetComponent<Rigidbody>();
        myRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        myRigidbody.useGravity = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        attractor.Attract(myTransform);
    }
}
