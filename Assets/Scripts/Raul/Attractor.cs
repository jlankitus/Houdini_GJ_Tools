using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attractor : MonoBehaviour
{
    public float attractionForce = 10f;

    private void OnTriggerStay(Collider other)
    {
        ApplyGravity(other.gameObject);
    }

    private void ApplyGravity(GameObject other)
    {
        GravityHandler gravHandler = other.GetComponent<GravityHandler>();

        // Ignore if object has no Rigidbody
        if (gravHandler == null)
            return;

        Vector3 gravDirection = (other.transform.position - transform.position).normalized;
        float gravAngle = Vector3.Angle(transform.position, other.transform.position);

        gravHandler.SetGravityVector(gravDirection * attractionForce);

    }
}
