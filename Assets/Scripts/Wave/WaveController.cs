using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using UnityEngine;

public class WaveController : NetworkBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private int totalWaves = 3;
    [SerializeField] private float waveDuration = 30f;
    [SerializeField] private float timeBetweenWaves = 15f;
    [SerializeField] private float spawnInterval = 0.5f;

    [Header("Synced Variables")]
    private readonly SyncVar<float> remainingWaveTime = new() ;
    private readonly SyncVar<int> currentWave = new();
    private readonly SyncVar<float> betweenWaveTime = new();

    [Header("References")]
    [SerializeField] private EnemySpawner enemySpawner;

    [Header("UI Access")]
    public float RemainingWaveTime => remainingWaveTime.Value;
    public int CurrentWave => currentWave.Value;
    public int TotalWaves => totalWaves;

    public float TimeBetweenWaves => betweenWaveTime.Value;

    private Coroutine spawnCoroutine;


    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine(ManageWaves());
    }
    private IEnumerator ManageWaves()
    {
        for (int wave = 1; wave <= totalWaves; wave++)
        {
            //wait for both players to be ready
            yield return StartCoroutine(WaitForPlayers());

            currentWave.Value = wave;
            remainingWaveTime.Value = waveDuration;

            spawnCoroutine = StartCoroutine(SpawnEnemies());
            while (remainingWaveTime.Value > 0f)
            {
                remainingWaveTime.Value -= Time.deltaTime;
                yield return null;
            }
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
            // Wave end, despawn all enemies
            enemySpawner.DespawnAllEnemies();

            //wait before starting next wave and set Timer to 0
            remainingWaveTime.Value = 0f;

            if (wave < totalWaves)
            {
                betweenWaveTime.Value = timeBetweenWaves;
                while (betweenWaveTime.Value > 0f)
                {
                    betweenWaveTime.Value -= Time.deltaTime;
                    yield return null;
                }
                betweenWaveTime.Value = 0f;
            }
        }
        Debug.Log("All waves completed!");
    }
    private IEnumerator SpawnEnemies()
    {
        float spawnTime = 0f;
        while (remainingWaveTime.Value > 0f)
        {
            spawnTime -= Time.deltaTime;

            if (spawnTime <= 0f)
            {
                enemySpawner.SpawnEnemy();
                spawnTime = spawnInterval;
            }
            yield return null;
        }
    }
    private IEnumerator WaitForPlayers()
    {
        while (PlayerTracker.Players.Count < 2)
        {
            yield return new WaitForSeconds(1f);
        }
    }

}
