using UnityEngine;
using FishNet.Object;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using FishNet.Demo.AdditiveScenes;

public class PlayerActions : NetworkBehaviour
{
    public int difficulty = 1;

    private int maxHealth = 3;
    private int currentHealth;

    private float dmg = 1f;
    private float critrate = 0.1f;
    private float critdmg = 1.2f;
    private float attackSpeed = 0.5f;
    private float moveSpeed;
    private float range = 10f;
    private float bulletSpeed = 10f;
    private float bulletRange = 10f;

    private List<Attackmodifire> attackmodifires;

    private bool canAttack = true;

    private Transform target;

    public LayerMask enemyLayer;

    private void Awake() 
    {
        attackmodifires = new List<Attackmodifire>();
        //attackmodifires.Add(Attackmodifire.Triple);
        //attackmodifires.Add(Attackmodifire.Behind);
    }

    private void Update() 
    {
        if(!IsOwner) return;
        if (canAttack)
        {
            targetEnemy();
            if (target != null)
            {
                canAttack = false;
                // Attack the target
                Vector3 directionToTarget = (target.position - transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                Attack(targetRotation);
                if (attackmodifires.Contains(Attackmodifire.Triple))
                {
                    Quaternion leftRotation = Quaternion.Euler(0, -15f, 0) * targetRotation;
                    Attack(leftRotation);
                    Quaternion rightRotation = Quaternion.Euler(0, 15f, 0) * targetRotation;
                    Attack(rightRotation);
                }
                if (attackmodifires.Contains(Attackmodifire.Behind))
                {
                    Vector3 reverseDirectionToTarget = -(target.position - transform.position).normalized;
                    Quaternion reverseTargetRotation = Quaternion.LookRotation(reverseDirectionToTarget);
                    Attack(reverseTargetRotation);
                    if (attackmodifires.Contains(Attackmodifire.Triple))
                    {
                        Quaternion reverseLeftRotation = Quaternion.Euler(0, -15f, 0) * reverseTargetRotation;
                        Attack(reverseLeftRotation);
                        Quaternion reverseRightRotation = Quaternion.Euler(0, 15f, 0) * reverseTargetRotation;
                        Attack(reverseRightRotation);
                    }
                }


                Invoke("ResetAttack", 1f / attackSpeed);
            }
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("EnemyProjectile"))
        {
            TakeDamage();
        }
    }
    public void TakeDamage()
    {
        Debug.Log("Player took damage");
        currentHealth -= difficulty;
        if (currentHealth <= 0)
        {
            //Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    public void IncreaseDmg( float amount)
    {
        dmg += amount;
    }
    public void IncreaseCritRate(float amount)
    {
        critrate += amount;
    }
    public void IncreaseCritDmg(float amount)
    {
        critdmg += amount;
    }
    public void IncreaseAttackSpeed(float amount)
    {
        attackSpeed += amount;
    }
    public void IncreaseMoveSpeed(float amount)
    {
        moveSpeed += amount;
    }
    public void IncreaseRange(float amount)
    {
        range += amount;
    }
    public void IncreaseBulletSpeed(float amount)
    {
        bulletSpeed += amount;
    }
    public void IncreaseBulletRange(float amount)
    {
        bulletRange += amount;
    }
    public void IncreaseMaxHP()
    {
        maxHealth += 1;
        currentHealth += 1;
    }

    public void AddAttackModifire(Attackmodifire modifire)
    {
        attackmodifires.Add(modifire);
    }

    private void targetEnemy()
    {
        target = null;
        //Catst a sphere to detect enemys in range
        Collider[] hitColl = Physics.OverlapSphere(transform.position, range, enemyLayer);
        //and return the enemys in range
        foreach (Collider enemy in hitColl)
        {
            // if no target is selected yet, select the first one
            if (target == null)
            {
                target = enemy.transform;
                continue;
            }
            if (Vector3.Distance(transform.position, enemy.transform.position) < Vector3.Distance(transform.position, target.position))
            {
                target = enemy.transform;
            }
        }
    }

    private void Attack(Quaternion direction)
    {
        float projDmg;
        if (Random.Range(0f, 1f) < critrate)
        {
            projDmg = dmg * critdmg;
        }
        else
        {
            projDmg = dmg;
        }

        //Instantiate a projectile and set its direction towards the target
        ProjectileSpawner.Instance.SpawnProjectileServer( transform.position, direction, bulletRange, bulletSpeed, projDmg, true);
    }
    private void ResetAttack()
    {
        canAttack = true;
    }

}
public enum Attackmodifire
{
    Triple,
    Behind,
    Explode,
    Piercing,
    Homing
}
