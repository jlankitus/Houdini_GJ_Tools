using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ThirdPersonMovement : MonoBehaviour
{
    private Rigidbody rigidbody;
    public float speed = 6f;
    public float turnSmoothTime = 0.1f;
    public Transform cameraTransform;
    public Transform GFXTransform;

    float turnSmoothVelocity;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Only Move on X and Z, since those are Forward and Sideways
        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            // Calculate character rotation based on Camera and WASD Movement
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float smoothedAngle = Mathf.SmoothDampAngle(GFXTransform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

            // Fake Character rotation by rotating ONLY the Graphics object inside ^^"
            // GFXTransform.forward = Vector3.forward * targetAngle;
            GFXTransform.localEulerAngles = new Vector3(0, targetAngle, 0);
            
            // Move the Parent the desired direction, with some movement dampening
            Vector3 moveDirection = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;

            rigidbody.velocity += (moveDirection.normalized * speed * Time.deltaTime);
        }
    }
}
