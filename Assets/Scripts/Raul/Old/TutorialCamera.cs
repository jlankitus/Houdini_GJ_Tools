using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialCamera : MonoBehaviour
{

    public GameObject target;
    public float xSpeed = 3.5f;
    float sensitivity = 17f;

    float minFov = 35;
    float maxFov = 100;
    
    public void OnCameraZoom(InputValue input)
    {
        // Get Input
        float zoomDirection = input.Get<float>();

        // Fetch current FieldOfView
        float fov = Camera.main.fieldOfView;
        // Add value to current Field of View
        fov += zoomDirection * -sensitivity;
        // Clamp accordingly
        fov = Mathf.Clamp(fov, minFov, maxFov);
        // Apply FoV
        Camera.main.fieldOfView = fov;
    }

    public void OnCameraMove(InputValue input)
    {

    }

    // Update is called once per frame
    void Update()
    {

        // ToDo: Adjust for new Input System
        if (Input.GetMouseButton(1))
        {
            transform.RotateAround(target.transform.position, transform.up, Input.GetAxis("Mouse X") * xSpeed);
            transform.RotateAround(target.transform.position, transform.right, -Input.GetAxis("Mouse Y") * xSpeed);
        }
        
    }
}