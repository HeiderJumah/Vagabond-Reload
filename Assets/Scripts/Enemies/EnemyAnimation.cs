using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    [Header("References")]
    private Animator animator;
    private EnemyAnimationState state;

    // cache hash ids
    private readonly int AnimationStateHash = Animator.StringToHash("AnimationState");

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetAnimationState(EnemyAnimationState newState)
    {
        if (state == newState) return;
        state = newState;
        animator.SetInteger(AnimationStateHash, (int)state);
    }


    public float GetAnimationTime()
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        return state.length;
    }

}
