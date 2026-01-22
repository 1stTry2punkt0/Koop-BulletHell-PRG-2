using FishNet.Connection;
using FishNet.Demo.AdditiveScenes;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class PlayerActions : NetworkBehaviour
{
    public int difficulty = 1;

    private int maxHealth = 3;
    readonly public SyncVar<int> currentHealth = new SyncVar<int>();
   // private int currentHealth;

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

    public readonly SyncVar<bool> IsAlive = new SyncVar<bool>(true);
   // public bool IsAlive {get; private set;} = true;

    private void Start() 
    {
        currentHealth.OnChange += OnHealthChanged;
    }
    private void Awake() 
    {
        attackmodifires = new List<Attackmodifire>();
        //attackmodifires.Add(Attackmodifire.Triple);
        //attackmodifires.Add(Attackmodifire.Behind);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        currentHealth.Value = maxHealth;
        IsAlive.Value = true;
    }

    public override void OnStartClient()
    {
        if (IsOwner)
        {
            UIManager.Instance.UpdateHealth(currentHealth.Value, maxHealth);

            LootManager.instance.playerActions = this;
            LootManager.instance.StartLevelUp();
        }
    }

    private void Update() 
    {
        if(!IsOwner || !IsAlive.Value) return;
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
    private void OnHealthChanged(int oldValue, int newValue, bool asServer)
    {
        if (IsOwner)
        {
            UIManager.Instance.UpdateHealth(newValue, maxHealth);
        }
    }

    [Server]
    public void ApplyServerDamage(int amount)
    {
        currentHealth.Value -= amount;
        if (currentHealth.Value <= 0)
        {
            currentHealth.Value = 0;
            Die();
        }
    }

    [Server]
    private void Die()
    {
        if(!IsAlive.Value) return;
        //Handle player death
        Debug.Log("Player Died");
        IsAlive.Value = false;
        DisableControls();
        CheckAllPlayerDead();
    }

    [Server]
    private void CheckAllPlayerDead()
    {
        bool allDead = true;
        foreach (var player in PlayerTracker.Players)
        {
            PlayerActions actions = player.GetComponent<PlayerActions>();
            if (actions != null && actions.IsAlive.Value)
            {
                allDead = false;
                break;
            }
        }
        if (allDead)
        {
            WaveController waveController = FindFirstObjectByType<WaveController>();
            if (waveController != null) 
                {
                waveController.EndGame();
                }

            // Trigger game over
            RpcShowEndscreen(false);
        }
    }
    [ObserversRpc]
    private void RpcShowEndscreen(bool isWin)
    {
        UIManager.Instance.ActivateEndScreen(false);
    }
    [Server]
    public void HealOnServer(int amount)
    {
        if (!IsAlive.Value)
        {
            IsAlive.Value = true;
            EnableControls();
        }
        currentHealth.Value += amount;
        if (currentHealth.Value > maxHealth)
        {
            currentHealth.Value = maxHealth;
        }
        //UIManager.Instance.UpdateHealth(currentHealth.Value, maxHealth);
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
        currentHealth.Value += 1;
        UIManager.Instance.UpdateHealth(currentHealth.Value, maxHealth);
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
        if(!IsAlive.Value) return;
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
        if(!IsAlive.Value) return;
        canAttack = true;
    }

    public void DisableControls()
    {
        canAttack = false;

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
            movement.enabled = false;
    }
    public void EnableControls()
    {
        canAttack = true;
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
            movement.enabled = true;
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
