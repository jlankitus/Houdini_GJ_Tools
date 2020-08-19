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
            transform.rotation = Quaternion.Euler(0, smoothedAngle, 0);

            Vector3 moveDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            // Unknown if this will behave as expected.
            rigidbody.MovePosition(transform.position + direction * moveSpeed * Time.deltaTime);

        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, a);
    }
}
