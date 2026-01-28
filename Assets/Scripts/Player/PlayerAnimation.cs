using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("References")]
    private Animator animator;
    private PlayerAnimationState state;
    // cache hash ids
    private readonly int AnimationStateHash = Animator.StringToHash("AnimState");
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    public void SetAnimationState(PlayerAnimationState newState)
    {
        if (state == newState) return;
        state = newState;
        animator.SetInteger(AnimationStateHash, (int)state);
    }
}
