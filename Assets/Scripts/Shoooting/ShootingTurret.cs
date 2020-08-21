using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShootingTurret : MonoBehaviour
{
    [SerializeField] private Transform bulletHole;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform barrelRotation;
    [SerializeField] private Transform shootTarget;
    [SerializeField] private float strength = .5f;
    [SerializeField] private Quaternion rotationOffset;
    [SerializeField] private float range = 5f;

    private List<GameObject> enemies;
    private float nextActionTime = 0.0f;
    public float bulletCooldown = 0.1f;
    
    public float enemyScanCycle = 1f;

    // COROUTINE VARIABLES. DO NOT TOUCH!
    private float lastBulletTime, lastScanTime;

    void Start()
    {
        enemies = GameObject.FindGameObjectsWithTag("Enemy").ToList();
    }
    void FixedUpdate()
    {
        if (shootTarget != null)
        {
            RotateBarrelToTarget();
            StartCoroutine(FireAtRate(bulletCooldown));
        }

        // Find enemies every Scan Cycle
        StartCoroutine(ScanForEnemies());
        
        if (shootTarget == null)
            LockOnEnemy();
    }

    //void FireAtRate(float rate)
    //{
    //    if (Time.time > nextActionTime ) 
    //    {
    //        nextActionTime += rate;
    //        Instantiate(bulletPrefab, bulletHole.transform);
    //    }
    //}

    IEnumerator FireAtRate(float rate)
    {
        // Check if we should wait a bit longer or not.
        if (Time.time - lastBulletTime <= bulletCooldown)
        {
            yield break;
        }

        // Reset the clock
        lastBulletTime = Time.time;

        // Invoke a bullet to charge at the Enemy
        Instantiate(bulletPrefab, bulletHole.transform);
    }

    private IEnumerator ScanForEnemies()
    {
        // Check if we should wait a bit longer or not.
        if (Time.time - lastScanTime <= enemyScanCycle)
        {
            yield break;
        }
        // Reset the clock
        lastScanTime = Time.time;
        
        // Refresh the list with new Enemies
        enemies = GameObject.FindGameObjectsWithTag("Enemy").ToList();

        // Reset the Shooting Target, so we don't accidentally end up shooting outside of our range
        shootTarget = null;
    }

    private void RotateBarrelToTarget()
    {
        var directionBetween = shootTarget.position - barrelRotation.position;
        // Debug.DrawRay(barrelRotation.position, directionBetween * 1000f);
        var targetRotation = Quaternion.LookRotation(directionBetween);
        targetRotation *= rotationOffset;
        barrelRotation.rotation = targetRotation;
    }

    private void LockOnEnemy()
    {
        
        for (int i = 0; i < enemies.Count; i++)
        {
            var currentEnemy = enemies[i];
            if (currentEnemy == null)
            {
                // Remove the enemy from the list, that way we have less to count through
                enemies.Remove(currentEnemy);
                // Reset our counter, since we need to check again the entry with this number
                i--;
                continue;
            }

            var distance = Vector3.Distance(transform.position, currentEnemy.transform.position);
            if (distance < range)
            {
                shootTarget = currentEnemy.transform;
                break;
            }
        }
    }
}
