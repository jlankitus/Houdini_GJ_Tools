using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartCounter : MonoBehaviour
{
    public List<Image> lifeImages = new List<Image>();
    private int lifeCount = 4;
    public Canvas gameCanvas;
    public Canvas loseCanvas;

    private GameLoop gameLoop;

    private void Awake()
    {
        gameLoop = FindObjectOfType<GameLoop>();
    }

    public void LoseLife()
    {
        lifeCount -= 1;
        lifeImages[lifeCount].enabled = false;
        if (lifeCount == 0)
        {
            if (!gameLoop.HasLeft)
            {
                OnYouLose(); 
            }
        }
    }

    private void OnYouLose()
    {
        gameCanvas.gameObject.SetActive(false);
        loseCanvas.gameObject.SetActive(true);
    }
}
