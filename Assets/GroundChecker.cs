using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    public bool onGround = false;
    public float distanceToGround = 0f;
    public Vector3 groundNormal = Vector3.zero;

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Planet" && !other.isTrigger)
            onGround = true;
    }
    private void OnTriggerStay(Collider other) {
        if (other.gameObject.tag == "Planet" && !other.isTrigger)
            onGround = true;
    }
    private void OnTriggerExit(Collider other) {
        if (other.gameObject.tag == "Planet" && !other.isTrigger)
            onGround = false;
    }

    private void FixedUpdate()
    {
        CalculateCollisionDistance();
    }

    private void CalculateCollisionDistance()
    {
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(transform.position, -transform.up, out hit, 10))
        {
            distanceToGround = hit.distance;
            groundNormal = hit.normal;
        }
        
    }
}
