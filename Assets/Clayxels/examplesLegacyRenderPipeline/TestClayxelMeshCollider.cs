using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestClayxelMeshCollider : MonoBehaviour
{
	Clayxels.ClayContainer clayxel;

    void Start(){
        // Here we initialize the clay container and we add the relevant components

        this.clayxel = this.GetComponent<Clayxels.ClayContainer>();
        this.clayxel.init();

        this.gameObject.AddComponent<MeshFilter>();
        this.gameObject.AddComponent<MeshRenderer>();
        this.gameObject.AddComponent<MeshCollider>();

        this.gameObject.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Clayxels/ClayxelBuiltInMeshShader"));

        // we'll disable the container update so that we can manually compute clay when we need it, once per frame
        this.clayxel.enabled = false;
    }

    void FixedUpdate(){
        // here we just randomize the rotation of a few clay objects, badly

        GameObject[] pivots = GameObject.FindGameObjectsWithTag("Player");
        for(int i = 0; i < pivots.Length; ++i){
            if(UnityEngine.Random.Range(0, 100) > 50){
                continue;
            }

            Transform trn = pivots[i].transform;
            Vector3 newRot = new Vector3(0.0f, 0.0f, Mathf.Lerp(trn.localEulerAngles.z, UnityEngine.Random.Range(-10.0f, 10.0f), 0.1f));
            if(newRot.z < -30.0f){
                newRot.z = -30.0f;
            }
            else if(newRot.z > 30.0f){
                newRot.z = 30.0f;
            }

            trn.localEulerAngles = newRot;
        }
        
        // inform the container we updated all clay objects and recompute the clay
        this.clayxel.forceUpdateAllSolids();
        this.clayxel.computeClay();

        // we generate a mesh at level of detail 50, fast and good enough for collisions
        int levelOfDetail = 10;
        Mesh mesh = this.clayxel.generateMesh(levelOfDetail, true);
        mesh.RecalculateNormals();
        
        // finally we visualize the mesh and set the collider
        this.gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
        this.gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
