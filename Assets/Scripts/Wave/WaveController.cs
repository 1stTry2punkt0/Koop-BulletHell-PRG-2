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
    [SerializeField] private float spawnInterval = 0.5f;
    [SerializeField] private List<WaveEnemySetter> waveEnemySetters;

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
        for (int wave = 1; wave <= totalWaves; wave++)
        {
            //wait for both players to be ready
            yield return StartCoroutine(WaitForPlayers());

            // Get enemy settings for the current wave
            WaveEnemySetter enemySetter = GetEnemiesForWave(wave);

            // Configure enemy spawner for the current wave
            enemySpawner.SetEnemyPool(enemySetter.enemyPrefabs);

            // Initialize wave variables
            currentWave.Value = wave;
            remainingWaveTime.Value = enemySetter.waveDuration;

            if(enemySetter.bossWave)
            {
                // Spawn only one enemy at the start of the wave
                enemySpawner.SpawnEnemy();
            }
            else if(!enemySetter.bossWave)
            {
                // Start spawning enemies at regular intervals
                spawnCoroutine = StartCoroutine(SpawnEnemies());
            }
            while (remainingWaveTime.Value > 0f)
            {
                // Update remaining wave time
                remainingWaveTime.Value -= Time.deltaTime;
                yield return null;
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
                enemySpawner.SpawnEnemy();
                spawnTime = spawnInterval;
            }
            yield return null;
        }
    }
    private IEnumerator WaitForPlayers()
    {
        // Wait until at least 2 players are connected
        while (PlayerTracker.Players.Count < 2)
        {
            yield return new WaitForSeconds(1f);
        }
    }

}
