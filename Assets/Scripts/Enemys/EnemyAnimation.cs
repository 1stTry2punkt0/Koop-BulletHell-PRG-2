using UnityEngine;
using FishNet.Object;
using FishNet.Component.Animating;

public class EnemyAnimation : NetworkBehaviour
{
    Animator animator;
    NetworkAnimator networkAnimator;

    private void Awake() 
    {
        animator = GetComponent<Animator>();
        networkAnimator = GetComponent<NetworkAnimator>();
    }

     private void Update() { }

    public void Run()
    {

    }

    public void Attack()
    {

    }

    public void Idle()
    {

    }

    public void Die()
    {

    }


}
