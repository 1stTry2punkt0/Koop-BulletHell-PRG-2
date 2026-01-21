using FishNet;
using FishNet.Object;
using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine.AI;

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
    private const float cameraHeight = 15f; // Height of the player's camera from ground

    private readonly List<NetworkObject> _spawnedEnemies = new(); // track spawned enemies

    #region Spawn Enemies
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

            // Subscribe to enemy Death to automatically unregister
            Enemy enemy = enemyInstance.GetComponent<Enemy>();
            if (enemy != null)
                enemy.OnDeathEvent += OnEnemyDeath;
        }

    }
    public Enemy LastSpawnedEnemy
    {
        get
        {
            if(_spawnedEnemies.Count == 0) return null;
            return _spawnedEnemies[_spawnedEnemies.Count - 1].GetComponent<Enemy>();
        }
    }
    #endregion

    #region Find Spawn 
    /// <summary>
    /// Uses player's camera parameters to determine if a point is visible to the player.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    private bool IsVisibleToPlayer(PlayerMovement player, Vector3 point)
    {
        // Convert point to player's local space
        Vector3 localPos = point -player.transform.position;

        // Calculate camera frustum dimensions at ground level
        float halfHeight = Mathf.Tan(Mathf.Deg2Rad * (player.CamFov / 2f)) * cameraHeight; 
        float halfWidth = halfHeight * player.CamAspect; 

        // Check if point is within frustum bounds
        return Mathf.Abs(localPos.x) <= halfWidth && Mathf.Abs(localPos.z) <= halfHeight;
    }

    /// <summary>
    /// Visualizes the player's camera rectangle and the spawn point for debugging purposes.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="point"></param>
    private void VisualizeCameraRectangle(PlayerMovement player, Vector3 point)
    {
        // Calculate camera frustum dimensions at ground level, same as in IsVisibleToPlayer
        float halfHeight = Mathf.Tan(Mathf.Deg2Rad * (player.CamFov / 2f)) * cameraHeight;
        float halfWidth = halfHeight * player.CamAspect;
        // Get center position
        Vector3 center = player.transform.position;
        // Debug draw rectangle on ground
        Vector3 bl = center + new Vector3(-halfWidth, 0f, -halfHeight); // get bottom left corner
        Vector3 br = center + new Vector3(halfWidth, 0f, -halfHeight); // bottom right
        Vector3 tr = center + new Vector3(halfWidth, 0f, halfHeight); // top right
        Vector3 tl = center + new Vector3(-halfWidth, 0f, halfHeight); // top left


        // Draw rectangle lines
        Debug.DrawLine(bl, br, Color.green, 1f); 
        Debug.DrawLine(br, tr, Color.green, 1f);
        Debug.DrawLine(tr, tl, Color.green, 1f);
        Debug.DrawLine(tl, bl, Color.green, 1f);

        // Draw line to spawn point
        bool isInside = IsVisibleToPlayer(player, point);
        // Color red if inside view, blue if outside
        Debug.DrawLine(point, point + Vector3.up * 2f, isInside ? Color.red : Color.blue, 1f);
    }

    /// <summary>
    /// Finds a valid spawn position around the specified player, ensuring it's outside the player's camera view and free of obstacles.
    /// </summary>
    /// <returns></returns>

    private Vector3 FindSpawn(PlayerMovement player)
    {
        Vector3 spawnPos = Vector3.zero;

        // Find a valid spawn position
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector3 direction = Random.onUnitSphere; // Choose a random direction
            direction.y = 0; // Keep spawn on the horizontal plane
            direction.Normalize();

            // Random distance within specified range
            float distance = Random.Range(spawnDistanceMin, spawnDistanceMax);
            Vector3 potentialPos = player.transform.position + direction * distance; // Calculate potential spawn position

            // Check if on NavMesh
            NavMeshHit hit;
            if (!NavMesh.SamplePosition(potentialPos, out hit, 2.0f, NavMesh.AllAreas)) // 2.0f is the max distance to sample
            {
                Debug.Log("Spawn position not on NavMesh, trying again.");
                continue; // Not on NavMesh, try again
            }

            bool isInView = false;

            // Check all Players' cameras to ensure spawn is outside their view
            foreach (var p in PlayerTracker.Players)
            {
                if (IsVisibleToPlayer(p, hit.position))
                {
                    // Position is in this player's view
                    isInView = true;
                    break;
                }
            }
            // Visualize the camera rectangle and spawn point for debugging
            VisualizeCameraRectangle(player, hit.position); 

            // If position is in any player's view, continue to next attempt
            if (isInView)
            {
                Debug.Log("Spawn position is in player's view, trying again.");
                continue;
            }


            // Check for obstacles
            if (!Physics.CheckSphere(hit.position, spawnRadiusCheck, obstacles))
            {
                spawnPos = hit.position;
                break;
            }
        }
        return spawnPos; // Return found position or Vector3.zero if none found
    }

    #endregion

    #region Despawn Enemy
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

    public void UnregisterEnemy(NetworkObject enemy)
    {
        _spawnedEnemies.Remove(enemy);
    }

    private void OnEnemyDeath(Enemy enemy)
    {
        if (enemy == null || enemy.NetworkObject == null)
            return;

        //Remove from spawned list 
        _spawnedEnemies.Remove(enemy.NetworkObject);
    }

    #endregion
}
