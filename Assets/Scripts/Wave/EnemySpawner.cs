using FishNet;
using FishNet.Object;
using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class EnemySpawner : NetworkBehaviour
{
    [Header("Enemy Prefab")]
    [SerializeField] private NetworkObject enemyPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnDistanceMin = 8f; // Min distance from player
    [SerializeField] private float spawnDistanceMax = 15f; // Max distance from player 
    [SerializeField] private LayerMask obstacles; // LayerMask for obstacles
    [SerializeField] private float spawnRadiusCheck = 1f; // Radius to check for obstacles
    [SerializeField] private int maxSpawnAttempts = 10; // Max attempts to find a valid spawn position

    private readonly List<NetworkObject> _spawnedEnemies = new List<NetworkObject>();

    public void SpawnEnemy()
    {
        if (!IsServerInitialized)
            return;
        var players = PlayerTracker.Players;
        if (players.Count == 0)
            return;
        PlayerMovement randomPlayer = players[Random.Range(0, players.Count)];

        Vector3 spawnPos = FindSpawn(randomPlayer);
        if (spawnPos != Vector3.zero)
        {
            NetworkObject enemyInstance = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            Spawn(enemyInstance);
            _spawnedEnemies.Add(enemyInstance);
        }

    }

    private Vector3 FindSpawn(PlayerMovement player)
    {
        Vector3 playerPos = player.transform.position;
        Camera playerCam = Camera.main;

        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector3 direction = Random.onUnitSphere;
            direction.y = 0; // Keep spawn on the horizontal plane
            direction.Normalize();
            float distance = Random.Range(spawnDistanceMin, spawnDistanceMax);
            Vector3 potentialPos = playerPos + direction * distance;
            // keep spawn outside player camera view
            Vector3 viewportPoint = playerCam.WorldToViewportPoint(potentialPos);
            if (!(viewportPoint.x < 0 || viewportPoint.x > 1 || viewportPoint.y < 0 || viewportPoint.y > 1))
                continue;

            // Raycast down to find ground level
            Ray ray = new Ray(potentialPos + Vector3.up * 10f, Vector3.down); 

            // Check for obstacles
            if (!Physics.CheckSphere(potentialPos, spawnRadiusCheck, obstacles))
                {
                    return potentialPos;
                }

        }
        return Vector3.zero; // Failed to find a valid position
    }

    public void DespawnAllEnemies()
    {
        // Despawn all spawned enemies
        foreach (var enemy in _spawnedEnemies)
        {
            if (enemy != null && enemy.IsSpawned)
            {
                Despawn(enemy);
            }
        }
        _spawnedEnemies.Clear();
    }
}
