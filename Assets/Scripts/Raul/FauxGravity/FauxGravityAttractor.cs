using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FauxGravityAttractor : MonoBehaviour
{
    public float gravity = -10f;
    public float bodyRotationSpeed = 50f;

    public void Attract(Transform body)
    {
        // Calculate Gravity Direction
        Vector3 gravityUp = (body.position - transform.position).normalized;
        // Get Current Up Direction for Attracted body
        Vector3 bodyUp = body.up;
        // Get RigidBody
        Rigidbody rb = body.GetComponent<Rigidbody>();
        // Add Gravity Force
        rb.AddForce(gravityUp * gravity);

        // Get the desired rotation difference
        Quaternion targetRotation = Quaternion.FromToRotation(bodyUp, gravityUp);
        // Add the current body rotation
        targetRotation *= body.rotation;
        // Apply the rotation to the Body with a Spherical Lerp (slerp)
        body.GetComponent<Rigidbody>().MoveRotation(Quaternion.Slerp(body.rotation, targetRotation, bodyRotationSpeed * Time.deltaTime));

    }
}
