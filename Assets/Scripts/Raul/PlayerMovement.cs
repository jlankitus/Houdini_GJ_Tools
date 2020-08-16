using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(GroundChecker))]
public class PlayerMovement : MonoBehaviour
{
    [HideInInspector] public GameObject activePlanet;

    public float speed = 4;
    public float JumpHeight = 1.2f;
    public float RotationSpeed = 90f;

    private Rigidbody rigidbody;
    
    float gravity = 100;

    private GroundChecker groundChecker;

    private float rotation = 0f;
    private float movement = 0f;


    void OnEnable()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.freezeRotation = true;
        groundChecker = GetComponentInChildren<GroundChecker>();
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
            rigidbody.AddForce(transform.up * 40000 * JumpHeight * Time.deltaTime);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Local Rotation
        transform.Rotate(0, RotationSpeed * rotation * Time.deltaTime, 0);

        // Player Movement
        // rigidbody.velocity = new Vector3(0, 0, movement);
        transform.Translate(0, 0, movement);
        
        //GroundControl
        
        // ApplyGravity();
        AdjustRotation();
        
        SmoothMovementAndRotation();
    }

    private void ApplyGravity()
    {
        Vector3 gravDirection = (transform.position - activePlanet.transform.position).normalized;
        if (!groundChecker.onGround)
        {
            rigidbody.AddForce(gravDirection * -gravity);
        }
    }

    private void AdjustRotation()
    {
        Quaternion toRotation = Quaternion.FromToRotation(transform.up, groundChecker.groundNormal) * transform.rotation;
        transform.rotation = toRotation;
    }

    private void SmoothMovementAndRotation()
    {
        // Smooth Position Movement
        transform.position = Vector3.Lerp(transform.position, transform.position, 0.1f);
        Vector3 gravDirection = Vector3.zero;
        if (activePlanet != null)
            gravDirection = (transform.position - activePlanet.transform.position).normalized;
        else
            gravDirection = (transform.position).normalized;

        // Smooth Rotation Movement
        Quaternion toRotation = Quaternion.FromToRotation(transform.up, gravDirection) * transform.rotation;
        transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, 0.1f);
    }


    public void ChangePlanetTo(GameObject newPlanet)
    {
        activePlanet = newPlanet;
    }

}
