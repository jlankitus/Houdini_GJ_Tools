using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretUpgradeInteraction : MonoBehaviour
{
    private TurretLevel currentSelectedTurret = null;

    private void Start()
    {
    }

    private void FixedUpdate()
    {
        UpdateCurrentlySelectedTurret();
    }
    

    Vector3 a, b;

    RaycastHit hitInfo;

    private void UpdateCurrentlySelectedTurret()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hitInfo, 500f, LayerMask.GetMask("Turret")))
        {
            currentSelectedTurret = hitInfo.collider.gameObject.GetComponent<TurretLevel>();
        }
    }

    private void OnDrawGizmos()
    {
        if (currentSelectedTurret)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(currentSelectedTurret.transform.position, currentSelectedTurret.transform.position + a);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(currentSelectedTurret.transform.position, currentSelectedTurret.transform.position + b);
        }
    }


    /*************************************
     * New Input System - Input Handling *
     *************************************/


    // Upgrade Turret Level
    public void OnUpgradeTurret() {
        if (currentSelectedTurret)
        {
            // Make sure we have a Collider and we are targeting the Planet Layer
            if (hitInfo.collider && hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Turret"))
            {
                currentSelectedTurret.LevelUp();
            }
            
        }
    }
}
