using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerGroundCheck : MonoBehaviour
{
    public LayerMask groundableLayer; 

    public bool isCurrentlyGrounded = false;

    private void OnTriggerEnter(Collider other)
    {
        if ((groundableLayer.value & 1 << other.gameObject.layer) == 1 << other.gameObject.layer)
        {
            isCurrentlyGrounded = true;
            
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if ((groundableLayer.value & 1 << other.gameObject.layer) == 1 << other.gameObject.layer)
        {
            isCurrentlyGrounded = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((groundableLayer.value & 1 << other.gameObject.layer) == 1 << other.gameObject.layer && gameObject.activeInHierarchy)
        {
            isCurrentlyGrounded = false;
        }
    }
}
