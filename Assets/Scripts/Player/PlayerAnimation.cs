using FishNet.Object;
using UnityEngine;

public class PlayerAnimation : NetworkBehaviour
{
    // References to Animator component and Players current State
    private Animator animator;
    private PlayerAnimationState currentState;

    // Cached hash to avoid repeated string lookup
    private static readonly int AniStateHash = Animator.StringToHash("AniState");

    // readonly access to the current animation state
    public PlayerAnimationState CurrentState => currentState;

    // Prevents state changes while Player is in a locked animation
    private bool isLocked => currentState == PlayerAnimationState.Death;

    private void Awake()
    {
        // get animator on initialzation 
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Set player animation state on server and sync with clients
    /// </summary>
    /// <param name="state"></param>
    /// <param name="forceChange">Ignore lock and  state checks</param>

    [Server]
    public void SetState(PlayerAnimationState state, bool forceChange = false)
    {
        // Ignore redunant state changes unless forced
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
    /// Unlocks animation state and forces player to Idle state
    /// </summary>
    public void Unlock()
    {
        SetState(PlayerAnimationState.Idle, true);
    }

}
