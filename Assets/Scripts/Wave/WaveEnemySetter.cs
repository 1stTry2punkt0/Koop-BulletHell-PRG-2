using UnityEngine;
using FishNet.Object;
using System.Collections.Generic;

[System.Serializable]
public class WaveEnemySetter
{
    /// <summary>
    /// Wave settings for each wave
    /// </summary>
    [Tooltip("Index of the wave")]
    public int waveIndex;

    [Tooltip("Enemies allowed in this wave")]
    public List<NetworkObject> enemyPrefabs;

    [Tooltip("Spawn only one enemy")]
    public bool bossWave;

    [Tooltip("Duration of the wave in seconds")]
    public float waveDuration = 30f;
}
