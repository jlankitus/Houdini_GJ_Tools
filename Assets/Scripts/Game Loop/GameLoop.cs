using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Kino.PostProcessing;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class GameLoop : Singleton<GameLoop>
{
    [SerializeField]
    private int towels;
    public int Towels
    {
        get { return towels; }
        set
        {
            towels = value;
            UpdateTowelUI();
        }
    }

    [SerializeField] private TextMeshProUGUI towelCount;
    [SerializeField] private VolumeProfile volumeProfile;
    private VolumeComponent glitch;
    private float glitchLength = .2f;
    public EnemySpawn enemySpawn;
    public OrbitCamera orbitCamera;
    public Transform characterTransform;
    public MovingSphere characterMovement;

    private void Start()
    {
        GetGlitchRef();
    }

    private void UpdateTowelUI()
    {
        towelCount.text = towels.ToString();
        if (towels % 3 == 0)
        {
            OnWaveComplete();
        }
    }

    private void OnWaveComplete()
    {
        var currentEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (currentEnemies.Length < 20)
        {
            enemySpawn.SpawnEnemy();
            enemySpawn.SpawnEnemy();
            enemySpawn.SpawnEnemy();
        }
        characterMovement.jumpHeight += 1;
    }

    private void GetGlitchRef()
    {
       foreach (var component in volumeProfile.components)
        {
            if (component.displayName == "glitch")
            {
                glitch = component;
            }
        }
    }

    public void AddTowel()
    {
        Towels += 1;
    }

    public void RemoveTowel()
    {
        Towels -= 1;
        glitch.active = true;
        StartCoroutine(StopGlitch());
    }

    IEnumerator StopGlitch()
    {
        yield return new WaitForSeconds(glitchLength);
        glitch.active = false;
    }
}
