using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyTestMesh : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MeshFilter mesh = GetComponent<MeshFilter>();
        Debug.Log(mesh.mesh.bounds.center);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
