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
    private TriggerGroundCheck _triggerGroundChecker;

    // Local rotation related
    public float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;
    private float currentJumpSpeed = 0f;
    private bool jumpButtonPressed = false, isDescending = false;
    private float totalTime = 0f;

    [SerializeField] float jumpInputRememberTime = 0.2f;
    private float _jumpInputRemember = 0;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        _triggerGroundChecker = GetComponentInChildren<TriggerGroundCheck>();
    }

    // Update is called once per frame
    void Update()
    {
        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
    }

    public void OnJump(InputValue input)
    {
        DoJump();
        /*if (!isJumping)
        {
            isJumping = true;
            // Offset the jump acceleration a bit
            t = 0.4f;
        }*/
    }

    private void DoJump()
    {
        _jumpInputRemember = jumpInputRememberTime;
        jumpButtonPressed = true;
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
            GFXTransform.GetComponent<Rigidbody>().MoveRotation(Quaternion.Slerp(rigidbody.rotation, targetRotation, 500f * Time.deltaTime));

            // Add Forward facing movement, based on Child orientation
            rigidbody.velocity += GFXTransform.forward * moveSpeed; //* Time.deltaTime;
            
        }


        // Handle Jumping
        _jumpInputRemember -= Time.fixedDeltaTime;
        Vector3 jumpVelocity = Vector3.zero;
        if (_triggerGroundChecker.isCurrentlyGrounded && _jumpInputRemember > 0)
        {
            jumpVelocity = GFXTransform.up * jumpSpeed;

            _triggerGroundChecker.isCurrentlyGrounded = false;
            _jumpInputRemember = 0;
        }

        rigidbody.velocity += jumpVelocity;
        // Add Jump Movement, based on Child orientation
        /*
        if (isJumping)
        {
            currentJumpSpeed = Mathf.Lerp(0, jumpSpeed, t);
            t += 3f * Time.deltaTime;

            if (t >= 0.99)
            {
                isJumping = false;
            }
        } else
        {
            currentJumpSpeed = 0;
        }
        

        rigidbody.velocity += GFXTransform.up * currentJumpSpeed;
        */
    }
    
}
