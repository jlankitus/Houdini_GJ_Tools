  using UnityEngine;
  using System.Collections;
  
  public class RandomObjectPlacer : MonoBehaviour {
  
      public int numObjects = 10;
      public GameObject prefabToPlace;
      public float radius;
  
      void Start() 
      {
          Vector3 center = transform.position;
          
          for (int i = 0; i < numObjects; i++)
          {
              Vector3 pos = RandomCircle(center, radius);
              Quaternion rot = Quaternion.FromToRotation(Vector3.forward, center-pos);
              Instantiate(prefabToPlace, pos, rot);
          }
      }
  
      Vector3 RandomCircle ( Vector3 center ,   float radius  ){
          float ang = Random.value * 360;
          Vector3 pos;
          pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
          pos.y = center.y + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
          pos.z = center.z;
          return pos;
      }
  }