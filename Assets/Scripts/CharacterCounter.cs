using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCounter : MonoBehaviour
{
    // public float recoverSpeed = 0.2f;
    public float angleRecoverSpeed = 10f;
    public float angleInclinationSpeed = 20f;

    // public Vector3 targetPositionDelta = Vector3.zero;
    public Vector3 targetRotationEulers = Vector3.zero;
    public bool isInclinating = true;
    private Rigidbody rigidbody;
    
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
        
    }

    private void FixedUpdate()
    {
        if (isInclinating)
        {
            rigidbody.MoveRotation(Quaternion.Slerp(rigidbody.rotation, rigidbody.rotation * Quaternion.Euler(targetRotationEulers), angleInclinationSpeed * Time.deltaTime));
            if ((transform.localEulerAngles - targetRotationEulers).magnitude <= 0.1f)
            {
                isInclinating = false;
                targetRotationEulers = Vector3.zero;
            }
        }
        else
        {
            if ((transform.localEulerAngles).magnitude >= 0.1f)
                rigidbody.MoveRotation(Quaternion.Slerp(rigidbody.rotation, Quaternion.Euler(Vector3.zero), angleRecoverSpeed * Time.deltaTime));
            else
                isInclinating = true;

        }
    }
}
