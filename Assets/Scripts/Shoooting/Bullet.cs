using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float speed = 2;
    private float lifeTime = 5.0f;

    private Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * 10, ForceMode.Impulse);
        StartCoroutine(KillBulletInSeconds(lifeTime));
    }
    
    /*
    IEnumerator KillForceInSeconds(float forceTime)
    {
        yield return new WaitForSeconds(forceTime);
        rb.
        yield return null;
    }
    */
    
    IEnumerator KillBulletInSeconds(float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }
}
