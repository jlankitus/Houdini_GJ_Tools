using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CheckIfEscaped : MonoBehaviour
{
    private bool isEscaped = false;
    private SphereCollider escapeRadius;

    private void Start()
    {
        escapeRadius = GetComponent<SphereCollider>();
    }
    private void Update()
    {
        // CheckIfLeftThePlanet(this.gameObject.transform.position, escapeRadius.radius);
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "Character") HasExcaped();
    }

    private void HasExcaped()
    {
       Debug.LogError("ESCAPED");
       FindObjectOfType<GameLoop>().OnEscape();
    }
}
