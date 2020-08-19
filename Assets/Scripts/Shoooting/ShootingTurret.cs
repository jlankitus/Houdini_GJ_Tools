using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingTurret : MonoBehaviour
{
    [SerializeField] private Transform bulletHole;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform barrelRotation;
    [SerializeField] private Transform shootTarget;
    [SerializeField] private float strength = .5f;
    [SerializeField] private Quaternion rotationOffset;

    // Update is called once per frame
    void Update()
    {
        rotateBarrelToTarget();
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            Instantiate(bulletPrefab, bulletHole.transform);
        }
    }

    private void rotateBarrelToTarget()
    {
        var directionBetween = shootTarget.position - barrelRotation.position;
        Debug.DrawRay(barrelRotation.position, directionBetween * 1000f);
        var targetRotation = Quaternion.LookRotation(directionBetween);
        targetRotation *= rotationOffset;
        barrelRotation.rotation = targetRotation;
        /*
        var targetRotation = Quaternion.LookRotation (shootTarget.position - barrelRotation.position);
        var str = Mathf.Min (strength * Time.deltaTime, 1);
        barrelRotation.rotation = Quaternion.Lerp (transform.rotation, targetRotation, str);
        */
    }
}
