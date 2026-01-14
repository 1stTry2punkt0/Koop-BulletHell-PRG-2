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
        enemyAnimation.Die();
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

        //enemyAnimation.Attack();
        canAttack = false;
        StartCoroutine(AttackCooldown(enemyType.attackCooldown));
    }

    private void MeleeAttack() 
    {
        //Deal dmg
    }

    private void RangedAttack()
    {
        Debug.Log("Ranged Attack");
        if (enemyType.boss)
        {
            // Shoot multiple projectiles in a spread
            int projectileCount = 5;
            float spreadAngle = 30f; // Total spread angle in degrees
            for (int i = 0; i < projectileCount; i++)
            {
                float angle = -spreadAngle / 2 + (spreadAngle / (projectileCount - 1)) * i;
                Quaternion rotation = Quaternion.Euler(0, angle, 0) * Quaternion.LookRotation(enemyMovement.target.position - transform.position);
                NetworkObject projectile = Instantiate(enemyType.projectile, transform.position + transform.forward * 1.5f, rotation);

                Bullet projectileData = projectile.GetComponent<Bullet>();
                projectileData.direction = rotation * Vector3.forward;
                projectileData.SetTag("EnemyProjectile");
                projectileData.SetLayer(LayerMask.NameToLayer("EnemyProjectile"));
                Spawn(projectile);
            }
        }
        else
        {
            // Shoot single projectile
            NetworkObject projectile = Instantiate(enemyType.projectile, transform.position + transform.forward + Vector3.up, Quaternion.LookRotation(enemyMovement.target.position - transform.position));
            

            Bullet projectileData = projectile.GetComponent<Bullet>();
            projectileData.direction = (enemyMovement.target.position - projectile.transform.position).normalized;
            projectileData.SetTag("EnemyProjectile");
            projectileData.SetLayer(LayerMask.NameToLayer("EnemyProjectile"));
            Spawn(projectile);
        }
    }

    private void ChargeAttack()
    {

    }

    IEnumerator AttackCooldown(float cooldown)
    {
        //enemyAnimation.Idle();
        yield return new WaitForSeconds(cooldown);
        canAttack = true;
        enemyMovement.canMove = true;
        //enemyAnimation.Run();
    }

    IEnumerator ClearBody(GameObject enemy)
    {
        yield return new WaitForSeconds(2f);
        Despawn(enemy);
    }
}
