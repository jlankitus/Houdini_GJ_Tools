using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraMovement : MonoBehaviour
{
    public CinemachineFreeLook freeCam;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            // Connect Mouse Axis to Cinemachine Camera
            freeCam.m_XAxis.m_InputAxisName = "Mouse X";
            freeCam.m_YAxis.m_InputAxisName = "Mouse Y";
        }
        else
        {
            // Disconnect Axis from Cinemachine Camera
            freeCam.m_XAxis.m_InputAxisName = "";
            freeCam.m_YAxis.m_InputAxisName = "";
            // Reset Values, so we don't have any residual movement.
            freeCam.m_XAxis.m_InputAxisValue = 0;
            freeCam.m_YAxis.m_InputAxisValue = 0;
        }
    }
}
