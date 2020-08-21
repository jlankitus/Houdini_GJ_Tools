using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartDrawManager : MonoBehaviour
{
    public GameObject HeartSprite;
    public int MaxHeartCount = 5;
    public int HeartCount = 5;
    public Vector2 HeartSize = new Vector2(50, 50);
    public Vector2 HeartDistance = new Vector2(100, -100);

    private GameObject[] hearts;

    // Start is called before the first frame update
    void OnEnable()
    {
        hearts = new GameObject[HeartCount];
        CreateHearts();
        // UpdateHearts();
    }

    private void CreateHearts()
    {
        for (int i = 0; i < MaxHeartCount; i++)
        {
            GameObject newHeart = Instantiate(HeartSprite);

            newHeart.transform.parent = transform;
            var rectTransform = newHeart.GetComponent<RectTransform>();
            rectTransform.localPosition = i * HeartDistance;
            rectTransform.sizeDelta = HeartSize;

            hearts[i] = newHeart;
        }
    }

    public void UpdateHearts()
    {
        for (int i = 0; i < MaxHeartCount; i++)
        {
            if (i < HeartCount)
            {
                // hearts[i].gameObject.active = true;
                hearts[i].SetActive(true);
            } else
            {
                hearts[i].SetActive(false);
            }
        }
    }

    
}
