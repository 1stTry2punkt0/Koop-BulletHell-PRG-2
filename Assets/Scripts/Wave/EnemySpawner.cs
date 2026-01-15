using FishNet;
using FishNet.Object;
using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class EnemySpawner : NetworkBehaviour
{
    [Header("Enemy Pool")]
    private List<NetworkObject> activeEnemyPool = new(); // List of enemy prefrabs to spawn

    [Header("Spawn Settings")]
    [SerializeField] private float spawnDistanceMin = 8f; // Min distance from player
    [SerializeField] private float spawnDistanceMax = 15f; // Max distance from player 
    [SerializeField] private LayerMask obstacles; // LayerMask for obstacles
    [SerializeField] private float spawnRadiusCheck = 1f; // Radius to check for obstacles
    [SerializeField] private int maxSpawnAttempts = 10; // Limit attempts to find valid spawn position

    private readonly List<NetworkObject> _spawnedEnemies = new(); // track spawned enemies

    /// <summary>
    /// Spawns an enemy from the active enemy pool at a valid position relative to a one of the player.
    /// </summary>
    public void SpawnEnemy(NetworkObject enemyPrefab)
    {
        // Ensure server-side execution
        if (!IsServerInitialized)
            return;
        // Get Player from PlayerTracker and check if there are any players
        var players = PlayerTracker.Players;
        if (players.Count == 0)
            return;
        // Pick a random player to spawn enemy near
        PlayerMovement randomPlayer = players[Random.Range(0, players.Count)];

        // Find a valid spawn position
        Vector3 spawnPos = FindSpawn(randomPlayer);
        // If a valid position is found, spawn the enemy
        if (spawnPos != Vector3.zero)
        {

            // Instantiate and spawn the picked enemy
            NetworkObject enemyInstance = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            Spawn(enemyInstance);
            // Add to spawned enemies list for tracking
            _spawnedEnemies.Add(enemyInstance);
        }

    }
    /// <summary>
    /// Finds a valid spawn position around the specified player, ensuring it's outside the player's camera view and free of obstacles.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    private Vector3 FindSpawn(PlayerMovement player)
    {
        // Get player position and their main camera
        Vector3 playerPos = player.transform.position;
        Camera playerCam = Camera.main;

        // Attempt to find a valid spawn position
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector3 direction = Random.onUnitSphere; // Choose a random direction
            direction.y = 0; // Keep spawn on the horizontal plane
            direction.Normalize();
            // Random distance within specified range
            float distance = Random.Range(spawnDistanceMin, spawnDistanceMax);
            Vector3 potentialPos = playerPos + direction * distance; // Calculate potential spawn position
            // keep spawn outside player camera view
            Vector3 viewportPoint = playerCam.WorldToViewportPoint(potentialPos); // Convert to viewport coordinates
            // Check if within camera view
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
    /// <summary>
    /// Despawn all spawned enemies after wave ends.
    /// </summary>
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
        // Clear the list after despawning
        _spawnedEnemies.Clear();
    }
}
