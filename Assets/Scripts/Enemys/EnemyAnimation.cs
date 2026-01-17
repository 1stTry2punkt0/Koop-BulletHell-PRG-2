using UnityEngine;
using FishNet.Object;
using FishNet.Component.Animating;

public class EnemyAnimation : NetworkBehaviour
{
    Animator animator;
    private EnemyAnimationState currentState;

    private static readonly int AniStateHash = Animator.StringToHash("AniState");

    public EnemyAnimationState CurrentState => currentState;

    // Lock to prevent overrides 
   private bool isLocked => currentState == EnemyAnimationState.Attack || currentState ==  EnemyAnimationState.Death;

    private void Awake() 
    {
        animator = GetComponent<Animator>();
    }

    [Server]
    public void SetState(EnemyAnimationState state, bool forceChange = false)
    {
        if(!forceChange && (currentState == state || isLocked))
            return;
        currentState = state;
        // send integar to clients 
        RpcSetAnimationState((int)state);
    }

    [ObserversRpc]
    private void RpcSetAnimationState(int state)
    {
        animator.SetInteger(AniStateHash, state);
    }

    public void Unlock()
    {
        SetState(EnemyAnimationState.Idle, true);
    }
}