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
    }
}
