using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FauxGravityChangeHandler : MonoBehaviour
{
    FauxGravityAttractor attractor;

    private void Start()
    {
        // Fetch the Attractor component from the current object
        attractor = GetComponent<FauxGravityAttractor>();
    }

    private void OnTriggerStay(Collider other)
    {
        // If the object that entered the Trigger should be attracted
        if (other.GetComponent<FauxGravityBody>())
        {
            // Set the attractor for that object to this GameObject.
            other.GetComponent<FauxGravityBody>().attractor = attractor;
        }
    }
}
