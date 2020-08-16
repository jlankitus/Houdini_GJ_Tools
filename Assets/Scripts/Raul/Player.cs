using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public GameObject Planet;
    public GameObject PlayerPlaceholder;

    public float speed = 4;
    public float JumpHeight = 1.2f;
    public float RotationSpeed = 90f;

    private Rigidbody rigidbody;


    float gravity = 100;
    bool OnGround = false;


    float distanceToGround;
    Vector3 Groundnormal;
    
    private Rigidbody rb;

    private float rotation = 0f;
    private float movement = 0f;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
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

            if (distanceToGround <= 0.2f)
            {
                OnGround = true;
            }
            else
            {
                OnGround = false;
            }
            
        }


        //GRAVITY and ROTATION

        Vector3 gravDirection = (transform.position - Planet.transform.position).normalized;

        if (OnGround == false)
        {
            rb.AddForce(gravDirection * -gravity);

        }

        //

        Quaternion toRotation = Quaternion.FromToRotation(transform.up, Groundnormal) * transform.rotation;
        transform.rotation = toRotation;



    }


    //CHANGE PLANET

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.transform != Planet.transform)
        {
            // Switch current Gravity effector to new Planet
            Planet = collision.transform.gameObject;

            // Adjust gravity Direction
            Vector3 gravDirection = (transform.position - Planet.transform.position).normalized;

            // Calculate and apply rotation
            Quaternion toRotation = Quaternion.FromToRotation(transform.up, gravDirection) * transform.rotation;
            transform.rotation = toRotation;

            // Reset current Velocity
            rb.velocity = Vector3.zero;
            // Apply new Gravity force
            rb.AddForce(gravDirection * gravity);
            
            PlayerPlaceholder.GetComponent<PlayerMovement>().ChangePlanetTo(Planet);

        }
    }
}

