using FishNet.Object;
using FishNet.Object.Synchronizing;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveController : NetworkBehaviour
{
    // Basic settings for waves
    [Header("Wave Settings")]
    [SerializeField] private int totalWaves = 3;
    [SerializeField] private float timeBetweenWaves = 15f;
    [SerializeField] private List<WaveEnemySetter> waveEnemySetters;
    private float currentSpawnInterval;

    // Variables that need to be synced across clients
    [Header("Synced Variables")]
    private readonly SyncVar<float> remainingWaveTime = new() ;
    private readonly SyncVar<int> currentWave = new();
    private readonly SyncVar<float> betweenWaveTime = new();

    // Script references
    [Header("References")]
    [SerializeField] private EnemySpawner enemySpawner;

    // UI Accessors to display wave info
    [Header("UI Access")]
    public float RemainingWaveTime => remainingWaveTime.Value;
    public int CurrentWave => currentWave.Value;
    public int TotalWaves => totalWaves;

    public float TimeBetweenWaves => betweenWaveTime.Value;

    // Coroutine reference for spawning enemies
    private Coroutine spawnCoroutine;
    private List<WeightedEnemy> currentEnemies;
    private int totalTickets;

    public override void OnStartServer()
    {
        base.OnStartServer(); 
        StartCoroutine(ManageWaves()); // Start managing waves on the server
    }

    private WaveEnemySetter GetEnemiesForWave(int wave)
    {
        // Find the appropriate WaveEnemySetter for the current wave
        WaveEnemySetter setter = null; 
        WaveEnemySetter fallbackSetter = null;

        foreach (var s in waveEnemySetters)
        {
            //  check for exact match of wave index
            if (s.waveIndex == wave)
            {
                setter = s; // Exact match found
            }
            else if (s.waveIndex < wave)
            {
                // Keep track of the highest waveIndex less than the current wave
                if (fallbackSetter == null || s.waveIndex > fallbackSetter.waveIndex)
                {
                    fallbackSetter = s; // Fallback in case no exact match is found
                }
            }
        }
        return setter != null ? setter : fallbackSetter; // Return exact match or fallback
    }
    private IEnumerator ManageWaves()
    {
        //wait for both players to be ready
        yield return StartCoroutine(WaitForPlayers());

        // One time delay before starting the first wave
        betweenWaveTime.Value = timeBetweenWaves;
        while (betweenWaveTime.Value > 0f)
        {
            // Update time between waves
            betweenWaveTime.Value -= Time.deltaTime;
            yield return null;
        }
        betweenWaveTime.Value = 0f;

        for (int wave = 1; wave <= totalWaves; wave++)
        {

            // Get enemy settings for the current wave
            WaveEnemySetter enemySetter = GetEnemiesForWave(wave);
           
            currentEnemies = enemySetter.enemies;
            totalTickets = 0; // Reset total tickets for the wave
            // Calculate total tickets for weighted random selection
            foreach (var enemy in currentEnemies)
            {
                totalTickets += enemy.tickets;
            }

            // Initialize wave variables
            currentWave.Value = wave;
            remainingWaveTime.Value = enemySetter.waveDuration;
            currentSpawnInterval = enemySetter.spawnInterval;

            if (enemySetter.bossWave)
            {
                // Spawn only one enemy at the start of the wave
                if (currentEnemies.Count > 0) 
                { 
                    NetworkObject bossPrefab = currentEnemies[0].enemyPrefab;
                    enemySpawner.SpawnEnemy(bossPrefab);

                    Enemy boss = enemySpawner.LastSpawnedEnemy;

                    if (enemySetter.useTimer)
                    {
                        // wave ends on timer or boss death
                        while (remainingWaveTime.Value > 0f && boss != null && boss.CurrentHealth > 0)
                        {
                            remainingWaveTime.Value -= Time.deltaTime;
                            yield return null;
                        }
                    }
                    else if (enemySetter.useBossHealth)
                    {
                        // wave ends when boss dies
                        while(boss != null && boss.CurrentHealth > 0f)
                        {
                            yield return null;
                        }
                    }
                }
            }
            else 
            {
                // Start spawning enemies at regular intervals
                spawnCoroutine = StartCoroutine(SpawnEnemies());

                // Timer for normla waves
                while (remainingWaveTime.Value > 0f)
                {
                    // Update remaining wave time
                    remainingWaveTime.Value -= Time.deltaTime;
                    yield return null;
                }
            }
            if (spawnCoroutine != null)
            {
                // Stop spawning enemies at the end of the wave
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
            // Wave end, despawn all remaining enemies
            enemySpawner.DespawnAllEnemies();

            //wait before starting next wave and set Timer to 0
            remainingWaveTime.Value = 0f;

            // If not the last wave, prepare for the next wave
            if (wave < totalWaves)
            {
                // Start countdown for next wave
                betweenWaveTime.Value = timeBetweenWaves;
                while (betweenWaveTime.Value > 0f)
                {
                    // Update time between waves
                    betweenWaveTime.Value -= Time.deltaTime;
                    yield return null;
                }
                // Reset time between waves
                betweenWaveTime.Value = 0f;
            }
        }
        Debug.Log("All waves completed!");
    }
    private IEnumerator SpawnEnemies()
    {
        // Spawn enemies at regular intervals during the wave
        float spawnTime = 0f;
        while (remainingWaveTime.Value > 0f)
        {
            spawnTime -= Time.deltaTime;

            if (spawnTime <= 0f)
            {
                // Spawn an enemy and reset spawn timer to the spawn in intervals
                var prefab = GetRandomEnemyPrefab();
                enemySpawner.SpawnEnemy(prefab);
                spawnTime = currentSpawnInterval;
            }
            yield return null;
        }
    }
    private IEnumerator WaitForPlayers()
    {
        // Wait until at least 2 players are connected
        while (PlayerTracker.Players.Count < 2)
        {
            yield return null;
        }
    }
    private NetworkObject GetRandomEnemyPrefab()
    {
        // Select a random enemy prefab based on weighted tickets
        int randomTicket = Random.Range(0, totalTickets);
        int cumulativeTickets = 0;
        foreach (var enemy in currentEnemies)
        {
            // Accumulate tickets to find the selected enemy
            cumulativeTickets += enemy.tickets;
            if (randomTicket < cumulativeTickets)
            {
                return enemy.enemyPrefab; // Return the selected enemy prefab
            }
        }
        return currentEnemies[0].enemyPrefab; // Fallback in case of an error
    }

}
