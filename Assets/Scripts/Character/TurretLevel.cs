using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretLevel : MonoBehaviour
{
    public int currentLevel = 1;
    public int maxLevel = 3;

    public GameObject[] TurretModels;

    private void Start()
    {
        // Warn the Developer that something might be missing early
        if (TurretModels.Length != maxLevel)
            Debug.LogError("There is a mismatch between Max Level and the current available turret Models.");
    }

    public void LevelUp()
    {
        // Increase current level
        currentLevel++;
        
        // Make sure we don't exceed max level
        if (currentLevel > maxLevel)
            currentLevel = maxLevel;
        else 
            // Update the model only if we are not at Max level already
            UpdateModel();
    }

    private void UpdateModel()
    {
        // Find and delete the Old models
        Transform child = transform.GetChild(0);

        GameObject.Destroy(child.gameObject);

        // Instantiate the new Model
        GameObject newTurretModel = Instantiate(TurretModels[currentLevel - 1], transform, false);
    }
}
