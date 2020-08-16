using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlanetSwitchChecker : MonoBehaviour
{
    public GameObject PlanetPrefab;

    private Rigidbody rigidbody;
    private Rigidbody planetRigidbody;
    private PlayerMovement playerMovement;
    private GameObject player;

    private void OnEnable()
    {
        player = transform.parent.gameObject;
        rigidbody = GetComponent<Rigidbody>();
        planetRigidbody = PlanetPrefab.GetComponent<Rigidbody>();
        playerMovement = player.GetComponent<PlayerMovement>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.transform.tag == "Planet")
        {
            if (collision.transform != PlanetPrefab.transform)
            {
                // Switch current Gravity effector to new Planet
                PlanetPrefab = collision.transform.gameObject;

                // Adjust gravity Direction
                Vector3 gravDirection = (transform.position - PlanetPrefab.transform.position).normalized;

                // Calculate and apply rotation
                Quaternion toRotation = Quaternion.FromToRotation(transform.up, gravDirection) * transform.rotation;
                transform.rotation = toRotation;

                // Reset current Velocity
                rigidbody.velocity = Vector3.zero;
                // Apply new Gravity force
                rigidbody.AddForce(gravDirection * planetRigidbody.mass);

                // Change Planet reference
                playerMovement.ChangePlanetTo(PlanetPrefab);
            }
        }
    }
}
