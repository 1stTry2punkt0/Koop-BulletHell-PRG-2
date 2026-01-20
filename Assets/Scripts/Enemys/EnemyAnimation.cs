using UnityEngine;
using FishNet.Object;
using FishNet.Component.Animating;

public class EnemyAnimation : NetworkBehaviour
{
    // References to Animator component and Enemies current State
    private Animator animator;
    private EnemyAnimationState currentState;

    // Cached hash to avoid repeated string lookup
    private static readonly int AniStateHash = Animator.StringToHash("AniState");

    // readonly access to the current animation state
    public EnemyAnimationState CurrentState => currentState;

    // Lock to prevent overrides 
    private bool isLocked => currentState == EnemyAnimationState.Attack || currentState == EnemyAnimationState.Death || 
                             currentState == EnemyAnimationState.ChargedWindup || currentState == EnemyAnimationState.ChargedAttack;


    private void Awake() 
    {
        // get animator on initialzation
        animator = GetComponent<Animator>();
    }
    /// <summary>
    /// Set Enemy animation state on server and sync with clients
    /// </summary>
    /// <param name="state"></param>
    /// <param name="forceChange">ignore lock and state checks</param>
    [Server]
    public void SetState(EnemyAnimationState state, bool forceChange = false)
    {
        // ignore redundant state changes unless forced
        if(!forceChange && (currentState == state || isLocked))
            return;

        currentState = state;
        // Update animation state on all clients
        RpcSetAnimationState((int)state);
    }
    /// <summary>
    /// Applies animation state to Animator on all observing clients
    /// </summary>
    /// <param name="state"></param>
    [ObserversRpc]
    private void RpcSetAnimationState(int state)
    {
        animator.SetInteger(AniStateHash, state);
    }
    /// <summary>
    /// Unlocks animation state and forces enemy to "Idle" state
    /// </summary>
    public void Unlock()
    {
        SetState(EnemyAnimationState.Idle, true);
    }
    public void UnlockCharge()
    {
        SetState(EnemyAnimationState.ChargedAttack, true);
    }
}