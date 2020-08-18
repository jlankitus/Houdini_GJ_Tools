using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ThirdPersonMovement : MonoBehaviour
{
    private Rigidbody rigidbody;
    public float moveSpeed = 6f;
    public float turnSmoothTime = 0.1f;
    public Transform cameraTransform;
    public Transform GFXTransform;

    float turnSmoothVelocity;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    Vector3 a = Vector3.zero;

    void FixedUpdate()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Only Move on X and Z, since those are Forward and Sideways
        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            rigidbody.MovePosition(rigidbody.position + transform.TransformDirection(direction) * moveSpeed * Time.deltaTime);
            
            // Calculate character rotation based on Camera and WASD Movement
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.localEulerAngles.y;
            float smoothedAngle = Mathf.SmoothDampAngle(GFXTransform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

            // float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;


            // Fake Character rotation by rotating ONLY the Graphics object inside ^^"
            // GFXTransform.forward = Vector3.forward * targetAngle;
            //if (transform.eulerAngles.x > 180 || transform.eulerAngles.y > 180 || transform.eulerAngles.z > 180)
            //    GFXTransform.localEulerAngles = new Vector3(0, targetAngle + transform.eulerAngles.y, 0);
            //else
            //    GFXTransform.localEulerAngles = new Vector3(0, targetAngle - transform.eulerAngles.y, 0);

            // Move the Parent the desired direction, with some movement dampening

            // Vector3 moveDirection = Quaternion.Euler(0, targetAngle, 0) * transform.eulerAngles;


            //rigidbody.velocity += (moveDirection.normalized * speed * Time.deltaTime);
            //rigidbody.AddForce(moveDirection.normalized * speed);

            /*
            Vector3 relativePos = direction - transform.position;
            a = relativePos;
            //Vector3 moveDirection = Quaternion.FromToRotation(transform.eulerAngles, direction);
            rigidbody.AddRelativeForce(direction * speed);
            */
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, a);
    }
}
