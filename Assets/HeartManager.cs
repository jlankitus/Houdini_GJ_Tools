using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartManager : MonoBehaviour
{
    public GameObject HeartSprite;
    public int HeartCount = 5;
    public Vector2 HeartSize = new Vector2(50, 50);
    public Vector2 HeartDistance = new Vector2(100, -100);

    // Start is called before the first frame update
    void Start()
    {
        DrawHearts();
    }

    public void DrawHearts()
    {
        CleanHearts();
        for (int heartIndex = 0; heartIndex < HeartCount; heartIndex++)
        {
            GameObject newHeart = Instantiate(HeartSprite);
            
            newHeart.transform.parent = transform;
            var rectTransform = newHeart.GetComponent<RectTransform>();
            rectTransform.localPosition = heartIndex * HeartDistance;
            rectTransform.sizeDelta = HeartSize;

        }
    }

    //private void Update()
    //{
    //    DrawHearts();
    //}

    private void CleanHearts()
    {
        while (transform.childCount > 0)
        {
            Destroy(transform.GetChild(0).gameObject);
        }
    }
}
