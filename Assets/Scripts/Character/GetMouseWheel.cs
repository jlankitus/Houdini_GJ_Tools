using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetMouseWheel : MonoBehaviour
{
    private float scale = .1f;

    public GameObject rotateMe;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        rotateMe.transform.Rotate(Vector3.up, Input.mouseScrollDelta.y * 10f);
    }
}
