using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] private OrbitCamera orbitCamera;
    [SerializeField] private Camera godModeCam;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject world;
    
    public float horizontalSpeed = 2.0F;
    public float verticalSpeed = 2.0F;

    private bool god = false;

    public void SwitchToGod()
    {
        orbitCamera.enabled = false;
        godModeCam.enabled = true;
        playerInput.enabled = false;
    }

    public void SwitchToPlayer()
    {
        orbitCamera.enabled = true;
        godModeCam.enabled = false;
        playerInput.enabled = true;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.G))
        {
            if (!god)
            {
                SwitchToGod();
                god = true;
            }
            else
            {
                SwitchToPlayer();
                god = false;
            }
        }

        if (god)
        {
            // THIS IS BAD, NOT WORKING AT ALL
            float h = horizontalSpeed * Input.GetAxis("Mouse X");
            float v = verticalSpeed * Input.GetAxis("Mouse Y");
            world.transform.Rotate(v, h, 0);
        }
    }
}
