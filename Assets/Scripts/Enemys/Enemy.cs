using UnityEngine;
using FishNet.Object;
using System.Collections;
using UnityEngine.VFX;

public class Enemy : NetworkBehaviour
{
    public EnemyType enemyType;
    private EnemyMovement enemyMovement;
    private EnemyAnimation enemyAnimation;

    private float currentHealth;

    public bool canAttack = true;

    [Header("Animation Settings")]
    [SerializeField] private float attackAniTime = 1; 


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
            TakeDamage(collision.collider.GetComponent<Bullet>().damage);
        }
    }

    [Server]
    public void TakeDamage(float damage)
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
        enemyAnimation.SetState(EnemyAnimationState.Death);
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

        enemyAnimation.SetState(EnemyAnimationState.Attack);
        StartCoroutine(UnlockAfterAnimation(attackAniTime));
        canAttack = false;
        StartCoroutine(AttackCooldown(enemyType.attackCooldown));
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
            int projectileCount = 5;
            float spreadAngle = 30f; // Total spread angle in degrees
            for (int i = 0; i < projectileCount; i++)
            {
                float angle = -spreadAngle / 2 + (spreadAngle / (projectileCount - 1)) * i;
                Quaternion rotation = Quaternion.Euler(0, angle, 0) * Quaternion.LookRotation(enemyMovement.target.position - transform.position);
                ProjectileSpawner.Instance.SpawnProjectileServer( transform.position + transform.forward * 1.5f, rotation, 10f, 10f, 1f, false);

            }
        }
        else
        {
            // Shoot single projectile
            ProjectileSpawner.Instance.SpawnProjectileServer(transform.position + Vector3.up, Quaternion.LookRotation(enemyMovement.target.position - transform.position), 10f, 10f, 1f, false);
        }
    }

    private void ChargeAttack()
    {

    }

    IEnumerator AttackCooldown(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        canAttack = true;
        enemyMovement.canMove = true;
        enemyAnimation.SetState(EnemyAnimationState.Run);

    }
    private IEnumerator UnlockAfterAnimation(float animationTime)
    {
        yield return new WaitForSeconds(animationTime);
        enemyAnimation.Unlock();
    }
    IEnumerator ClearBody(GameObject enemy)
    {
        yield return new WaitForSeconds(2f);
        Despawn(enemy);
    }
}
