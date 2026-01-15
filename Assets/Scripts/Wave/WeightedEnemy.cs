using FishNet.Object;
using UnityEngine;

[System.Serializable]
public class WeightedEnemy
{
    /// <summary>
    /// Ticket system to weight enemy selection for spawning
    /// </summary>
    [Tooltip("Enemy prefab to spawn")]
    public NetworkObject enemyPrefab;
    [Tooltip("Weight of the enemy for random selection")]
    public int tickets = 1;
}
