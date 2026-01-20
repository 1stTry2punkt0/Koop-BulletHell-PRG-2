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

    private GameObject chargeConeVisualInstance;

    [Header("Animation Settings")]
    [SerializeField] private float attackAniTime = 1;

    private void Awake()
    {
        currentHealth = enemyType.health;
        enemyMovement = GetComponent<EnemyMovement>();
        enemyAnimation = GetComponent<EnemyAnimation>();

        if(enemyType.attackType == AttackType.Charge && enemyType.chargeConeVisualPrefab != null )
        {
            chargeConeVisualInstance = Instantiate(enemyType.chargeConeVisualPrefab, transform);
            UpdateChargeConeVisual();
            chargeConeVisualInstance.SetActive(false);
        }
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
        if (collision.collider.CompareTag("PlayerProjectile"))
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
        canAttack = false;
        // Handle attack logic here
        switch (enemyType.attackType)
        {
            case AttackType.Melee:
                // Implement melee attack logic
                MeleeAttack();
                enemyAnimation.SetState(EnemyAnimationState.Attack);
                StartCoroutine(UnlockAfterAnimation(attackAniTime));
                StartCoroutine(AttackCooldown(enemyType.attackCooldown));
                break;
            case AttackType.Ranged:
                // Instantiate and shoot projectile towards player
                RangedAttack();
                enemyAnimation.SetState(EnemyAnimationState.Attack);
                StartCoroutine(UnlockAfterAnimation(attackAniTime));
                StartCoroutine(AttackCooldown(enemyType.attackCooldown));
                break;
            case AttackType.Charge:
                // Implement charge attack logic
                ChargeAttack();
                enemyAnimation.SetState(EnemyAnimationState.ChargedWindup);
                break;
        }

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
                ProjectileSpawner.Instance.SpawnProjectileServer(transform.position + transform.forward * 1.5f, rotation, 10f, 10f, 1f, false);

            }
        }
        else
        {
            // Shoot single projectile
            ProjectileSpawner.Instance.SpawnProjectileServer(transform.position + Vector3.up, Quaternion.LookRotation(enemyMovement.target.position - transform.position), 10f, 10f, 1f, false);
        }
    }

    [Server]
    private void ChargeAttack()
    {
        if (!enemyMovement.target)
            return;

        StartCoroutine(ChargeAttackRoutine());
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

    private bool IsPlayerInCone(Transform target, float range, float angle)
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.magnitude > range)
            return false;

        float dotAngle = Vector3.Angle(transform.forward, direction);
        return dotAngle <= angle * 0.5f;
    }

    [Server]
    private IEnumerator ChargeAttackRoutine()
    {
        RpcShowChargeCone(true);

        yield return new WaitForSeconds(enemyType.chargeWindupTime);

        enemyAnimation.UnlockCharge();

        DealConeDamage();

        yield return new WaitForSeconds(1f);
        RpcShowChargeCone(false);

        enemyAnimation.Unlock();
        StartCoroutine(AttackCooldown(1f));
    }

    [Server]
    private void DealConeDamage()
    {
        foreach (PlayerMovement player in PlayerTracker.Players)
        {
            if (!player)
                continue;

            if (IsPlayerInCone(player.transform, enemyType.chargeConeRange, enemyType.chargeConeAngle))
            { 
                player.ChangeHealth((int)enemyType.chargeDamage);
            }
        }
    }

    [ObserversRpc]
    private void RpcShowChargeCone(bool show)
    {
        if(!chargeConeVisualInstance)
            return;

        chargeConeVisualInstance.SetActive(show);
        UpdateChargeConeVisual();
    }

    private void UpdateChargeConeVisual()
    {
        if (!chargeConeVisualInstance) return;

        float range = enemyType.chargeConeRange;
        float angle = enemyType.chargeConeAngle;

        // Radius at the end of the cone
        float radius = range * Mathf.Tan(angle * 0.5f * Mathf.Deg2Rad);

        chargeConeVisualInstance.transform.localScale =
            new Vector3(
                radius * 2f,   // width
                radius * 2f,   // height
                range  * 2f        // length 
            );

        chargeConeVisualInstance.transform.localPosition = Vector3.zero;
        chargeConeVisualInstance.transform.localRotation = Quaternion.identity;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (enemyType == null) return;
        Debug.Log("Drawing cone gizmo");
        // Draw cone in front of enemy
        Vector3 pos = transform.position;
        Vector3 forward = transform.forward;
        float range = enemyType.chargeConeRange;
        float angle = enemyType.chargeConeAngle;

        // Draw line from center to cone edges
        Vector3 leftDir = Quaternion.Euler(0, -angle * 0.5f, 0) * forward;
        Vector3 rightDir = Quaternion.Euler(0, angle * 0.5f, 0) * forward;

        Gizmos.color = new Color(1f, 0f, 0f, 0.5f); // semi-transparent red
        Gizmos.DrawLine(pos, pos + leftDir * range);
        Gizmos.DrawLine(pos, pos + rightDir * range);

        // Draw arc to visualize the cone
        int steps = 20;
        Vector3 previousPoint = pos + leftDir * range;
        for (int i = 1; i <= steps; i++)
        {
            float lerpAngle = -angle * 0.5f + (angle / steps) * i;
            Vector3 dir = Quaternion.Euler(0, lerpAngle, 0) * forward;
            Vector3 nextPoint = pos + dir * range;
            Gizmos.DrawLine(previousPoint, nextPoint);
            previousPoint = nextPoint;
        }
    }
#endif

}
