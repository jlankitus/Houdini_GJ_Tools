using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FauxGravityAttractor : MonoBehaviour
{
    
    // If the gravity of this section is only pushing the player down
    [Tooltip("Set to True if the planet is NOT round and you want the Attractor to be facing only 1 way")]
    public bool FixedDirection = false;

    [Tooltip("The force with which to Attracto objects affected")]
    public float gravity = -100f;

    [Tooltip("The Speed at which Objects attracted should rotate with the feet on the floor")]
    public float bodyRotationSpeed = 50f;

    public void Attract(Transform attractedTransform)
    {
        Vector3 gravityUp;
        if (!FixedDirection)
        {
            // Calculate Gravity Direction
            gravityUp = (attractedTransform.position - base.transform.position).normalized;
        } else
        {
            gravityUp = attractedTransform.up;
        }
        // Get Current Up Direction for Attracted body
        Vector3 bodyUp = attractedTransform.up;
        // Get RigidBody
        Rigidbody rb = attractedTransform.GetComponent<Rigidbody>();
        // Add Gravity Force
        rb.AddForce(gravityUp * gravity);

        // Get the desired rotation difference
        Quaternion targetRotation = Quaternion.FromToRotation(bodyUp, gravityUp);
        // Add the current body rotation
        targetRotation *= attractedTransform.rotation;
        // Apply the rotation to the Body with a Spherical Lerp (slerp)
        attractedTransform.GetComponent<Rigidbody>().MoveRotation(Quaternion.Slerp(attractedTransform.rotation, targetRotation, bodyRotationSpeed * Time.deltaTime));

    }
}
