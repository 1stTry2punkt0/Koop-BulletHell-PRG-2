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
}

public enum AttackType
{
    Charge,
    Ranged,
    Melee,
    Boss
}