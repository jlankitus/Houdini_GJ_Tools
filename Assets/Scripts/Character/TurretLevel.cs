﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretLevel : MonoBehaviour
{
    public int currentLevel = 1;
    public int maxLevel = 3;

    public GameObject[] TurretModels;

    public Transform[] TransformsToTransfer;
    public int[] TurretCosts;

    public GameLoop gameLoop;

    private void Start()
    {
        // Warn the Developer that something might be missing early
        if (TurretModels.Length != maxLevel)
            Debug.LogError("There is a mismatch between Max Level and the current available turret Models.");

        gameLoop = Object.FindObjectsOfType<GameLoop>()[0];
        
        // Create a Copy of the Transforms to take into account
        TransformsToTransfer = (Transform[]) TransformsToTransfer.Clone();
    }

    public void LevelUp()   
    {
        if (currentLevel < maxLevel)
        {
            if (gameLoop.Towels >= TurretCosts[currentLevel])
            {
                gameLoop.Towels -= TurretCosts[currentLevel];
                
                // Increase current level
                currentLevel++;
                if (currentLevel > maxLevel)
                    currentLevel = maxLevel;
                // Make sure we don't exceed max level
                // Update the model only if we are not at Max level already
                UpdateModel();

            }
        }
        
    }

    private void UpdateModel()
    {
        // Find and delete the Old models
        Transform child = transform.GetChild(0);
        GameObject.Destroy(child.gameObject);
        
        // Instantiate the new Model
        GameObject newTurretModel = Instantiate(TurretModels[currentLevel - 1], transform, false);

        // Update all the new Transforms, based on name
        for (int i = 0; i < TransformsToTransfer.Length; i++) { 
            Transform newTransform = (Transform) newTurretModel.transform.Find(TransformsToTransfer[i].name);

            newTransform.localPosition = TransformsToTransfer[i].localPosition;
            newTransform.localRotation = TransformsToTransfer[i].localRotation;
            newTransform.localScale = TransformsToTransfer[i].localScale;

            // Update Reference to Transforms
            TransformsToTransfer[i] = newTransform;
        }
    }
}
