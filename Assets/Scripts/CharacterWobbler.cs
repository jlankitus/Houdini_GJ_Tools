using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterWobbler : MonoBehaviour
{

    public float wobbleSpeed = 0.2f;
    public float targetMinDistance = 0.2f;
    public float maxDistanceFromParent = 3f;
    public float maxRotationFromForward = 30f;
    public Vector3 targetPositionDelta = Vector3.zero;
    public Vector3 targetRotationDelta = Vector3.zero;
    // Start is called before the first frame update
    private float distanceTraveled;
    void Start()
    {
        distanceTraveled = targetPositionDelta.magnitude;
    }

    // Update is called once per frame
    void Update()
    {
        if (distanceTraveled >= targetPositionDelta.magnitude)
        {
            Debug.Log("Hello!");
            distanceTraveled = 0f;
            // Generate a new position to head for
            targetPositionDelta = new Vector3(Random.RandomRange(-maxDistanceFromParent, maxDistanceFromParent), Random.RandomRange(-maxDistanceFromParent, maxDistanceFromParent), Random.RandomRange(-maxDistanceFromParent, maxDistanceFromParent));
            targetRotationDelta = new Vector3(Random.RandomRange(-maxRotationFromForward, maxRotationFromForward), 0, Random.RandomRange(-maxRotationFromForward, maxRotationFromForward));
        }
        
    }

    private void FixedUpdate()
    {
        distanceTraveled += wobbleSpeed * Time.deltaTime;
        transform.localPosition += targetPositionDelta * wobbleSpeed * Time.deltaTime;
        if (transform.localPosition.magnitude > maxDistanceFromParent)
            targetPositionDelta *= -1;
        if (transform.localEulerAngles.magnitude > maxRotationFromForward)
            targetRotationDelta *= -1;
        transform.localEulerAngles += targetRotationDelta * wobbleSpeed * Time.deltaTime;
    }
}
