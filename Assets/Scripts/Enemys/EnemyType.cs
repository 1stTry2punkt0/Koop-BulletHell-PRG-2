using UnityEngine;

[CreateAssetMenu(fileName = "EnemyType", menuName = "Scriptable Objects/EnemyType")]
public class EnemyType : ScriptableObject
{
    public float speed;
    public int health;
    public int damage;
    public bool boss;
}

public enum AttackType
{
    Charge,
    Ranged,
    Melee
}