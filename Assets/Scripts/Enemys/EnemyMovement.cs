using UnityEngine;
using FishNet.Object;

public class NewNetworkBehaviourTemplate : NetworkBehaviour
{
    private float speed;
    private gameObject target;


     private void Awake() { }

     private void Update() { }

    private void MoveTowardsTarget() 
    {
        if (target != null)
        {
            Vector3 direction = (target.transform.position - transform.position).normalized;
            //Check for obstacles in movement path
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, 1f))
            {
                //move around obstacle

                return;
            }

            transform.position += direction * speed * Time.deltaTime;
        }
    }

    private void FindTarget()
    {
        // Logic to find and set the target

    }
}
