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
    private float lastBulletTime;

    void Start()
    {
        enemies = GameObject.FindGameObjectsWithTag("Enemy").ToList();
    }
    void Update()
    {
        if (shootTarget != null)
        {
            RotateBarrelToTarget();
            StartCoroutine(FireAtRate(bulletCooldown));
        }
        else
        {
            FindEnemy();
            enemies = GameObject.FindGameObjectsWithTag("Enemy").ToList();
        }
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

        if (Time.time - lastBulletTime <= bulletCooldown)
        {
            yield break;
        }

        lastBulletTime = Time.time;

        Instantiate(bulletPrefab, bulletHole.transform);
        
    }

    private void RotateBarrelToTarget()
    {
        var directionBetween = shootTarget.position - barrelRotation.position;
        // Debug.DrawRay(barrelRotation.position, directionBetween * 1000f);
        var targetRotation = Quaternion.LookRotation(directionBetween);
        targetRotation *= rotationOffset;
        barrelRotation.rotation = targetRotation;
    }

    private void FindEnemy()
    {
        if(enemies.Count > 0)
        {
            foreach (var enemy in enemies)
            {
                if (enemy != null)
                {
                    var distance = Vector3.Distance(transform.position,enemy.transform.position);
                    if (distance < range)
                    {
                        shootTarget = enemy.transform;
                        break;
                    }
                }
            }
        }
    }
}
