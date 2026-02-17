using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("References")]
    private Animator animator;
    private PlayerAnimationState state;
    // cache hash ids
    private readonly int AnimationStateHash = Animator.StringToHash("AnimState");

    // Prevents state changes while Player is in a locked animation
    private bool isLocked => state == PlayerAnimationState.Attack || state == PlayerAnimationState.StrongAttack || state == PlayerAnimationState.Die;


    public PlayerAnimationState CurrentState => state;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    public void SetAnimationState(PlayerAnimationState newState, bool forceChange = false)
    {
        if (!forceChange && (state == newState || isLocked)) return;
        state = newState;
        animator.SetInteger(AnimationStateHash, (int)state);
    }

    public float GetAnimationTime()
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        return state.normalizedTime;
    }

}
