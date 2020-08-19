using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FauxPlayerController : MonoBehaviour
{

    public float moveSpeed = 15;
    public float jumpSpeed = 30f;
    public Transform mainCameraTransform;
    public Transform GFXTransform;

    private Vector3 moveDir;
    private Rigidbody rigidbody;

    // Local rotation related
    public float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;
    private float currentJumpSpeed = 0f;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
    }

    public void OnJump(InputValue input)
    {
        if (input.isPressed)
            currentJumpSpeed = jumpSpeed;
    }
    
    private void FixedUpdate()
    {
        if (moveDir.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg + mainCameraTransform.localEulerAngles.y; //- transform.eulerAngles.y;

            Vector3 yRotation = Vector3.up * targetAngle * 50f * Time.fixedDeltaTime;
            Quaternion deltaRotation = Quaternion.Euler(yRotation);
            Quaternion targetRotation = rigidbody.rotation * deltaRotation;

            // Set Rotation of the Child
            GFXTransform.GetComponent<Rigidbody>().MoveRotation(Quaternion.Slerp(rigidbody.rotation, targetRotation, 50f * Time.deltaTime));

            // Get current Position as reference, since we are adding a Delta
            Vector3 newPosition = rigidbody.position;
            // Add Forward facing movement, based on Child orientation
            newPosition += GFXTransform.forward * moveSpeed * Time.deltaTime;
            // Add Jump Movement, based on Child orientation
            newPosition += GFXTransform.up * currentJumpSpeed * Time.deltaTime;
            // Apply total Movement to rigidbody
            rigidbody.MovePosition(newPosition);

            currentJumpSpeed = Mathf.Lerp(currentJumpSpeed, 0f, 0.2f);
        }
    }
    
}
