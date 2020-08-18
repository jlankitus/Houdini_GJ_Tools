using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityHandler : MonoBehaviour
{
    private CharacterController controller;
    
    public float turnSmoothTime = 0.1f;

    Vector3 gravityForce = Vector3.zero;
    float turnSmoothVelocity;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    public float myAngle = -90f;
    public Vector3 offset = Vector3.zero;
    // Update is called once per frame
    void Update()
    {
        if (gravityForce.magnitude >= 0.1f)
        {
            Vector3 gravityDirection = gravityForce.normalized;
            Vector3 adjustedGravity = new Vector3(gravityDirection.x + offset.x, gravityDirection.y + offset.y, gravityDirection.z + offset.z);
            Quaternion rotation = Quaternion.LookRotation(gravityDirection);
            //transform.rotation = rotation;
            transform.up = gravityDirection;

            Vector3 moveDirection = rotation * -Vector3.forward;
            controller.Move(moveDirection.normalized * gravityForce.magnitude * Time.deltaTime);

            //transform.rotation = Quaternion.Euler(adjustedGravity);
            // transform.rotation = Quaternion.FromToRotation(transform.up, adjustedGravity);
            // transform.rotation = rotation;
            //transform.rotation = new Quaternion(rotation.x, rotation.y * 0, rotation.z, 1);
        }


    }

    public void SetGravityVector(Vector3 newGravity)
    {
        gravityForce = newGravity;
    }
}
