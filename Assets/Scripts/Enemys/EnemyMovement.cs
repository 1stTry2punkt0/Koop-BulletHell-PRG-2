using UnityEngine;
using FishNet.Object;
using UnityEngine.AI;
using System.Runtime.CompilerServices;
using UnityEngine.InputSystem.XR.Haptics;

public class EnemyMovement : NetworkBehaviour
{
    private EnemyType typeData;
    public Transform target;
    private NavMeshAgent agent;
    public bool canMove = true;

    private EnemyAnimation enemyAnimation;

    private void Awake() 
    { 
        typeData = GetComponent<Enemy>().enemyType;
        agent = GetComponent<NavMeshAgent>();
        enemyAnimation = GetComponent<EnemyAnimation>();

        agent.speed = typeData.speed;
        agent.stoppingDistance = typeData.attackRange - 2f;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        TimeManager.OnTick += TimeManager_OnTick;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        TimeManager.OnTick -= TimeManager_OnTick;
    }


    private void TimeManager_OnTick()
    {
        if (!IsServerInitialized || NetworkObject == null || !NetworkObject.IsSpawned) return;
        FindTarget();
        MoveTowardsTarget();
    }

    [Server]
    private void MoveTowardsTarget() 
    {
        if (target != null)
        {
            Vector3 direction = (target.transform.position - transform.position).normalized;
            if (!enemyAnimation.CurrentState.Equals(EnemyAnimationState.Attack) && !enemyAnimation.CurrentState.Equals(EnemyAnimationState.Death)
             && !enemyAnimation.CurrentState.Equals(EnemyAnimationState.ChargedWindup) && !enemyAnimation.CurrentState.Equals(EnemyAnimationState.ChargedAttack))
            {
                if(canMove)
                    enemyAnimation.SetState(EnemyAnimationState.Run);
                else
                    enemyAnimation.SetState(EnemyAnimationState.Idle);
            }
            if (!canMove)
            {
                enemyAnimation.SetState(EnemyAnimationState.Idle);
                return;
            }
            agent.SetDestination(target.position);
        }
    }

    private void FindTarget()
    {
        // Logic to find and set the target
        foreach ( PlayerMovement player in PlayerTracker.Players)
        {
            if (target == null)
            {
                target = player.transform;
            }
            else if (Vector3.Distance(transform.position, player.transform.position) < Vector3.Distance(transform.position, target.position))
            {
                target = player.transform;
            }
        }
    }

    public float DistanceToTarget()
    {
        if (target == null)
            return Mathf.Infinity;

        return Vector3.Distance(transform.position, target.position);
    }

}
