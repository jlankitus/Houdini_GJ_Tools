using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialCamera : MonoBehaviour
{

    public GameObject target;
    public float xSpeed = 3.5f;
    float sensitivity = 17f;

    float minFov = 35;
    float maxFov = 100;


    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButton(1))
        {

            transform.RotateAround(target.transform.position, transform.up, Input.GetAxis("Mouse X") * xSpeed);
            transform.RotateAround(target.transform.position, transform.right, -Input.GetAxis("Mouse Y") * xSpeed);

        }

        //ZOOM

        float fov = Camera.main.fieldOfView;
        fov += Input.GetAxis("Mouse ScrollWheel") * -sensitivity;
        fov = Mathf.Clamp(fov, minFov, maxFov);
        Camera.main.fieldOfView = fov;

    }
}