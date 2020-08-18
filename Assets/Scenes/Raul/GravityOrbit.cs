using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityOrbit : MonoBehaviour
{

    // The Center that all objects orbit
    public float Gravity;

    // If the gravity of this section is only pushing the player down
    public bool FixedDirection; 

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<GravityCtrl>())
        {
            // If this object has a Gravity Script, set this as the Planet
            other.GetComponent<GravityCtrl>().Gravity = this.GetComponent<GravityOrbit>();
        }
    }
}
