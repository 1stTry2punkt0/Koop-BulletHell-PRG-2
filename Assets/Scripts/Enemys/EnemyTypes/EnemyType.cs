using FishNet.Object;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyType", menuName = "Scriptable Objects/EnemyType")]
public class EnemyType : ScriptableObject
{
    public float speed;
    public float health;
    public int damage = 1;
    public AttackType attackType;
    public float attackRange;
    public float attackCooldown;
    public NetworkObject loot;
    public NetworkObject projectile;
    public bool boss;

    [Header("Charge Attack Settings")]
    public float chargeWindupTime = 3f;
    public float chargeConeAngle = 10f;
    public float chargeConeRange = 5f;
    public float chargeDamage = 2f; 

    public GameObject chargeConeVisualPrefab;
}

public enum AttackType
{
    Charge,
    Ranged,
    Melee,
    Boss
}