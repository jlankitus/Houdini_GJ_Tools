using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    public GameObject enemyPrefab;
    private Vector3 spawnRange;

    public void SpawnEnemy()
    {
        spawnRange = Random.insideUnitSphere * 100f;
        Instantiate(enemyPrefab, spawnRange, Quaternion.identity);
    }
}
