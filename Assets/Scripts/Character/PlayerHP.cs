using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHP: MonoBehaviour
{
    public int Health = 5;
    public float immunitySeconds = 1.5f;
    public HeartDrawManager heartDrawManager;

    public Renderer PlayerRenderer;
    public Material immunityMaterial, normalMaterial;

    private float lastDamaged;

    public GameObject gameCanvas;
    public GameObject loseCanvas;

    private void OnCollisionStay(Collision collision)
    {
        StartCoroutine(ApplyDamage(collision));
    }


    private void OnCollisionEnter(Collision collision)
    {
        StartCoroutine(ApplyDamage(collision));
    }

    
    IEnumerator ApplyDamage(Collision collision)
    {
        // Only get affected by Enemy Collisions
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {

            // Only get affected if not in Immunity window
            // Check if we should wait a bit longer or not.
            if (Time.time - lastDamaged <= immunitySeconds)
            {
                yield break;
            }
            // Reset the clock
            lastDamaged = Time.time;

            // Decrease health, and update hearts
            Health--;
            heartDrawManager.HeartCount = Health;
            heartDrawManager.UpdateHearts();

            // Die if needed
            if (Health <= 0)
            {
                Die();
            }
        }
    }

    private void FixedUpdate()
    {
        if (Time.time - lastDamaged <= immunitySeconds)
        {
            PlayerRenderer.material = immunityMaterial;
        } else
        {
            PlayerRenderer.material = normalMaterial;
        }
    }

    private void Die()
    {
        Debug.LogError("U DED");
        GetComponent<Collider>().enabled = false;
        gameCanvas.SetActive(false);
        loseCanvas.SetActive(true);
    }

    private void Start()
    {
        heartDrawManager.HeartCount = Health;
        heartDrawManager.UpdateHearts();
    }
}
