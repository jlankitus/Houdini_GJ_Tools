using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public GameObject Planet;
    public GameObject PlayerPlaceholder;

    public float speed = 4;
    public float JumpHeight = 1.2f;
    public float RotationSpeed = 90f;

    private Rigidbody rigidbody;


    float gravity = 100;

    float distanceToGround;
    Vector3 Groundnormal;

    private Rigidbody rb;
    private GroundChecker groundChecker;

    private float rotation = 0f;
    private float movement = 0f;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        groundChecker = GetComponent<GroundChecker>();
    }

    public void OnMovement(InputValue input)
    {
        rotation = input.Get<Vector2>().x;
        movement = input.Get<Vector2>().y;
    }

    public void OnJump(InputValue input)
    {
        if (input.isPressed)
        {
            rb.AddForce(transform.up * 40000 * JumpHeight * Time.deltaTime);
        }
    }

    // Update is called once per frame
    void Update()
    {

        // Local Rotation
        transform.Rotate(0, RotationSpeed * rotation * Time.deltaTime, 0);

        //MOVEMENT
        transform.Translate(0, 0, movement);
        
        //GroundControl

        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(transform.position, -transform.up, out hit, 10))
        {
            distanceToGround = hit.distance;
            Groundnormal = hit.normal;
        }


        //GRAVITY and ROTATION

        Vector3 gravDirection = (transform.position - Planet.transform.position).normalized;

        if (!groundChecker.onGround)
        {
            rb.AddForce(gravDirection * -gravity);
        }

        //

        Quaternion toRotation = Quaternion.FromToRotation(transform.up, Groundnormal) * transform.rotation;
        transform.rotation = toRotation;



    }


    
}
