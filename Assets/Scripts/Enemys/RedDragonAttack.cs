using UnityEngine;
using FishNet.Object;
using System.Collections;

public class RedDragonAttack : NetworkBehaviour
{
    private Coroutine attackRoutine;

    [SerializeField] private EnemyType typeData;
    [SerializeField] private EnemyMovement enemyMovement;
    [SerializeField] private Enemy enemy;

    public void PerformAttack()
    {
        // Implement Red Dragon specific attack logic here
        Debug.Log("Red Dragon performs a fiery attack!");
        int attackType = Random.Range(0, 4);
        switch (attackType)
        {
            case 0:
                attackRoutine = StartCoroutine(SpiralAttack());
                break;
            case 1:
                RingAttack();
                break;
            case 2:
                attackRoutine = StartCoroutine(BeamAttack());
                break;
            case 3:
                SpreadAttack();
                break;
        }

    }

    IEnumerator SpiralAttack()
    {
        float angle = 0f;
        float duration = 0f;
        while (duration <= 2f)
        {
            Quaternion rotation = Quaternion.Euler(0, angle, 0) * Quaternion.LookRotation(enemyMovement.target.position - transform.position);
            ProjectileSpawner.Instance.SpawnProjectileServer(transform.position + transform.forward * 1.5f, rotation, 10f, 10f, 1f, false);
            angle += 30f; // Increase angle for spiral effect
            duration += 0.2f;
            yield return new WaitForSeconds(0.2f);
        }
        StartCoroutine(enemy.AttackCooldown(typeData.attackCooldown));
    }

    private void RingAttack()
    {
        // Shoot multiple projectiles in a spread
        int projectileCount = 12;
        float spreadAngle = 360; // Total spread angle in degrees
        for (int i = 0; i < projectileCount; i++)
        {
            float angle = -spreadAngle / 2 + (spreadAngle / (projectileCount - 1)) * i;
            Quaternion rotation = Quaternion.Euler(0, angle, 0) * Quaternion.LookRotation(enemyMovement.target.position - transform.position);
            ProjectileSpawner.Instance.SpawnProjectileServer(transform.position + transform.forward * 1.5f, rotation, 10f, 10f, 1f, false);
        }
        StartCoroutine(enemy.AttackCooldown(typeData.attackCooldown));
    }

    IEnumerator BeamAttack()
    {
        float duration = 0f;
        while (duration <= 1.4f)
        {
            ProjectileSpawner.Instance.SpawnProjectileServer(transform.position + Vector3.up, Quaternion.LookRotation(enemyMovement.target.position - transform.position), 10f, 10f, 1f, false);
            duration += 0.2f;
            yield return new WaitForSeconds(0.2f);
        }
        StartCoroutine(enemy.AttackCooldown(typeData.attackCooldown));
    }

    private void SpreadAttack()
    {
        // Shoot multiple projectiles in a spread
        int projectileCount = Random.Range(2,6);
        float spreadAngle = 30f; // Total spread angle in degrees
        for (int i = 0; i < projectileCount; i++)
        {
            float angle = -spreadAngle / 2 + (spreadAngle / (projectileCount - 1)) * i;
            Quaternion rotation = Quaternion.Euler(0, angle, 0) * Quaternion.LookRotation(enemyMovement.target.position - transform.position);
            ProjectileSpawner.Instance.SpawnProjectileServer(transform.position + transform.forward * 1.5f, rotation, 10f, 10f, 1f, false);

        }
        StartCoroutine(enemy.AttackCooldown(typeData.attackCooldown));
    }

}
