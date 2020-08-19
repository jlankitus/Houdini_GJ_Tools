using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.InputSystem;

public class ClickToPlaceObject : MonoBehaviour
{
    public GameObject[] placeableObjects = new GameObject[3];
    private GameObject currentObjectToGenerate = null;
    
    private GameObject objectGhostToPlace = null;

    private float mouseWheelRotation;

    private void Start()
    {
        if (placeableObjects.Length > 0)
            currentObjectToGenerate = placeableObjects[0];
    }

    private void FixedUpdate()
    {
        if (objectGhostToPlace != null)
        {
            MoveCurrentObjectToMouse();
            RotateFromMouseWheel();
        }   
    }



    Vector3 a, b;

    RaycastHit hitInfo;

    private void MoveCurrentObjectToMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hitInfo, LayerMask.GetMask("Planet")))
        {
            if (hitInfo.collider != objectGhostToPlace)
            {
                objectGhostToPlace.transform.position = hitInfo.point;
                objectGhostToPlace.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
            }

        }
    }

    private void OnDrawGizmos()
    {
        if (objectGhostToPlace)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(objectGhostToPlace.transform.position, objectGhostToPlace.transform.position + a);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(objectGhostToPlace.transform.position, objectGhostToPlace.transform.position + b);
        }
    }

    private void RotateFromMouseWheel()
    {
        mouseWheelRotation += Input.mouseScrollDelta.y;
        objectGhostToPlace.transform.Rotate(Vector3.up, mouseWheelRotation * 10f);
    }


    /*************************************
     * New Input System - Input Handling *
     *************************************/
    public void OnCreateNewTurret()
    {
        // Instantiate new Object
        if (objectGhostToPlace != null)
        {
            Destroy(objectGhostToPlace);
        }
        else
        {
            objectGhostToPlace = Instantiate(currentObjectToGenerate);
            objectGhostToPlace.GetComponent<Collider>().enabled = false;
        }
        // Rotate it based on Mouse Scroll
        RotateFromMouseWheel();

        // Move it to mouse position
        MoveCurrentObjectToMouse();
    }

    public void OnConfirmBuild()
    {
        if (objectGhostToPlace)
        {
            // Make sure we have a Collider and we are targeting the Planet Layer
            if (hitInfo.collider && hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Planet"))
            {
                // Make sure the Raycast hitpoint is on the Planet Surface, or close enough
                if ((hitInfo.collider.transform.position - objectGhostToPlace.transform.position).magnitude <= hitInfo.collider.transform.localScale.magnitude * hitInfo.collider.GetComponent<SphereCollider>().radius + 0.1)
                {
                    // Place the turret and enable the Collider
                    objectGhostToPlace.GetComponent<Collider>().enabled = true;
                    // Remove the reference, so we don't move it again
                    objectGhostToPlace = null;
                }
            }
        }
    }
    

    // Switch Selected Object to Place
    public void OnChangeToObject1() { currentObjectToGenerate = placeableObjects[0]; }
    public void OnChangeToObject2() { currentObjectToGenerate = placeableObjects[1]; }
    public void OnChangeToObject3() { currentObjectToGenerate = placeableObjects[2]; }
}
