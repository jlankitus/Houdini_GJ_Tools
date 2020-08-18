using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ThirdPersonMovement : MonoBehaviour
{
    private CharacterController controller;
    public float speed = 6f;
    public float turnSmoothTime = 0.1f;
    public Transform cameraTransform;
    public Transform GFXTransform;

    float turnSmoothVelocity;

    private void Start()
    {
        controller = GetComponent<CharacterController>(); 
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Only Move on X and Z, since those are Forward and Sideways
        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            //float smoothedAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            Debug.Log(transform.rotation);
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
            //GFXTransform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
            Debug.Log(targetAngle);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDirection.normalized * speed * Time.deltaTime);
        }
    }
}
