using UnityEngine;
using FishNet.Object;
using System.Collections;

public class Enemy : NetworkBehaviour
{
    public EnemyType enemyType;
    private EnemyMovement enemyMovement;
    private EnemyAnimation enemyAnimation;

    private float currentHealth;

    public bool canAttack = true;



    private void Awake() 
    {
        currentHealth = enemyType.health;
        enemyMovement = GetComponent<EnemyMovement>();
        enemyAnimation = GetComponent<EnemyAnimation>();
    }

     private void Update() 
    {
        if (canAttack && enemyMovement.DistanceToTarget() <= enemyType.attackRange)
        {
            enemyMovement.canMove = false;
            AttackPlayer();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("PlayerProjectile"))
        {
            //TakeDamage(collision.collider.GetComponent<PlayerProjectile>().damage);
        }
    }

    [Server]
    public void TakeDamage(int damage)
    {
        // Handle damage logic here
        currentHealth -= damage;

        if (enemyType.boss)
        {
            // Update boss health bar UI
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    [Server]
    public void Die()
    {
        // Handle death logic here
        EnemyAnimation.Die();
        NetworkObject lootDrop = Instantiate(enemyType.loot, transform.position, Quaternion.identity);
        ClearBody(gameObject);
    }

    [Server]
    private void AttackPlayer()
    {
        // Handle attack logic here
        switch(enemyType.attackType)
        {
            case AttackType.Melee:
                // Implement melee attack logic
                MeleeAttack();
                break;
            case AttackType.Ranged:
                // Instantiate and shoot projectile towards player
                RangedAttack();
                break;
            case AttackType.Charge:
                // Implement charge attack logic
                ChargeAttack();
                break;
        }

        enemyAnimation.Attack();
        canAttack = false;
        AttackCooldown(enemyType.attackCooldown);
    }

    private void MeleeAttack() 
    {
        //Deal dmg
    }

    private void RangedAttack()
    {
        if (enemyType.boss)
        {
            // Shoot multiple projectiles in a spread
        }
        else
        {
            // Shoot single projectile
        }
    }

    private void ChargeAttack()
    {

    }

    IEnumerator AttackCooldown(float cooldown)
    {
        enemyAnimation.Idle();
        yield return new WaitForSeconds(cooldown);
        canAttack = true;
        enemyMovement.canMove = true;
        enemyAnimation.Run();
    }

    IEnumerator ClearBody(GameObject enemy)
    {
        yield return new WaitForSeconds(2f);
        Despawn(enemy);
    }
}
