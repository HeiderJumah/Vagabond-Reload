using System.Collections;
using UnityEngine;

public class Ghost : Enemy
{
    [Header("References")]
    [SerializeField] private LayerMask playerMask;

    [Header("Hover Settings")]
    [SerializeField] private float baseHoverHeight = 0f;
    [SerializeField] private float hoverRange = 1f;
    [SerializeField] private float hoverSpeed = 2f;

    protected override void ChasePlayer()
    {
        if (!canMove)
            return;

        // when Player spotted, chase player and stop patrolling
        if (isPatrolling)
        {
            StopCoroutine(patrolRoutine);
            isPatrolling = false;
        }

        enemyAnimation.SetAnimationState(EnemyAnimationState.Move);
        // allow vertical movement
        Vector3 target = playerTransform.position;
        float height = baseHoverHeight;
        height += Mathf.Sin(Time.time * hoverSpeed) * hoverRange;
        target.y += height;
        Vector3 direction = (target - transform.position).normalized;
        transform.position += direction * enemyType.speed * Time.deltaTime;

    }

    protected override IEnumerator PatrolRoutine()
    {
        while (isPatrolling && !isDead)
        {
            // choose random point around its spawn point in patrolling range to move to
            Vector2 randomCircle = Random.insideUnitCircle * enemyType.patrollingRange;
            patrolTarget = originalPosition + new Vector3(randomCircle.x, 0, randomCircle.y);

            // fallback if random point is unreachable
            float patrolTimer = 0f;

            // store spawn height 
            float startY = transform.position.y;

            // wait short time before moving to new position 
            // do not include vertical (y) since bat is moving up and down 
            while (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(patrolTarget.x, 0, patrolTarget.z))
                > 0.1f && patrolTimer < 5f && !isDead)
            {
                Vector3 direction = GetMoveDirection(patrolTarget);
                transform.position += direction * enemyType.speed * Time.deltaTime;

                // update vertical position for the bat 
                float Offset = Mathf.Sin(Time.time * hoverSpeed) * hoverRange;
                transform.position = new Vector3(transform.position.x, startY + Offset, transform.position.z);

                enemyAnimation.SetAnimationState(EnemyAnimationState.Move);
                patrolTimer += Time.deltaTime;

                yield return null;
            }

            // wait a short time after reaching patrol point before patrolling again
            enemyAnimation.SetAnimationState(EnemyAnimationState.Idle);
            yield return new WaitForSeconds(enemyType.patrolWaitTime);
        }
    }

    public void GhostAttack()
    {
        Vector3 center = transform.position + transform.forward * Type.attackRange;

        Collider[] hitCollider = Physics.OverlapSphere(center, Type.attackRange, playerMask);

        foreach (Collider col in hitCollider)
        {
            PlayerActions player = col.GetComponent<PlayerActions>();
            if (player != null)
            {
                player.OnTakeDamage(Type.damage);
                player.ConfusedForward(6f);
                VariantEffects(player);
            }
        }
    }

    private void VariantEffects(PlayerActions player)
    {
        switch(Type.variant)
        {
            case Variant.One:
                player.SlowedForward(6f);
                break;
            case Variant.Two:
                player.BurnedStatus(5f);
                break;
            case Variant.Three:
                player.PoisonedStatus(4f);
                break;
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (Type == null)
            return;


        Vector3 center = transform.position + transform.forward * Type.attackRange;

        // Draw a semi-transparent red sphere
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawSphere(center, Type.attackRange);
    }

}
