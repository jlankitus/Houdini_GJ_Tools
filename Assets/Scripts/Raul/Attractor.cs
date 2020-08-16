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
        Rigidbody rigidbody = other.GetComponent<Rigidbody>();

        // Ignore if object has no Rigidbody
        if (rigidbody == null)
            return;

        Vector3 gravDirection = (other.transform.position - transform.position).normalized;

        rigidbody.AddForce(gravDirection * - attractionForce);
    }
}
