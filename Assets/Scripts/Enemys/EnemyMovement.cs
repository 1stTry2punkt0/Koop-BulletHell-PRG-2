using UnityEngine;
using FishNet.Object;
using UnityEngine.AI;

public class EnemyMovement : NetworkBehaviour
{
    private EnemyType typeData;
    public Transform target;
    private NavMeshAgent agent;
    public bool canMove = true;

    private void Awake() 
    { 
        typeData = GetComponent<Enemy>().enemyType;
        agent = GetComponent<NavMeshAgent>();

        agent.speed = typeData.speed;
        agent.stoppingDistance = typeData.attackRange - 2f;
    }


    private void Update()
    {
        FindTarget();
        MoveTowardsTarget();
    }

    [Server]
    private void MoveTowardsTarget() 
    {
        if (target != null)
        {
            Vector3 direction = (target.transform.position - transform.position).normalized;
            if (!canMove) return;
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
