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
            //Quaternion.Euler(0f, 0f, 0f) * -Vector3.up
            
            //float targetAngleX = Mathf.Atan2(gravityForce.x, -gravityForce.y) * Mathf.Rad2Deg;
            float targetAngleX = Mathf.Atan2(Mathf.Abs(gravityForce.z), gravityForce.y) * Mathf.Rad2Deg;
            // targetAngleX = 0f;
            float targetAngleZ = Mathf.Atan2(Mathf.Abs(gravityForce.y), Mathf.Abs(gravityForce.x)) * Mathf.Rad2Deg + myAngle;
            //float targetAngleY = Mathf.Atan2(gravityForce.z, -gravityForce.y) * Mathf.Rad2Deg;
            //float targetAngleZ = Mathf.Atan2(gravityForce.z, -gravityForce.x) * Mathf.Rad2Deg;
            //float targetAngleZ = Mathf.Atan2(gravityForce.z, gravityForce.y) * Mathf.Rad2Deg;
            //float targetAngleZ = 0f;
            //float smoothedAngle = Mathf.SmoothDampAngle(transform.eulerAngles.z, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

            //transform.rotation = Quaternion.Euler(0f, smoothedAngle, 0f);
            //transform.rotation = Quaternion.Euler(targetAngleX, 0f, targetAngleZ);
            //transform.rotation = Quaternion.Euler(gravityForce.x + offset.x, gravityForce.y + offset.y, gravityForce.z + offset.z);
            Vector3 adjustedGravity = new Vector3(gravityForce.x + offset.x, gravityForce.y + offset.y, gravityForce.z + offset.z);
            Quaternion rotation = Quaternion.LookRotation(adjustedGravity, adjustedGravity);
            transform.rotation = new Quaternion(rotation.x, rotation.y * 0, rotation.z, 1);
            //transform.rotation = Quaternion.Euler(targetAngleX, 0f, 0f);
            //transform.rotation = Quaternion.Euler(targetAngleX, targetAngleY, targetAngleZ);

            //Vector3 moveDirection = Quaternion.Euler(targetAngleX, 0f, targetAngleZ) * -Vector3.up;
            //controller.Move(moveDirection.normalized * gravityForce.magnitude * Time.deltaTime);
        }
        

    }

    public void SetGravityVector(Vector3 newGravity)
    {
        gravityForce = newGravity;
    }
}
