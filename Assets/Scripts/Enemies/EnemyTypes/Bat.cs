using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Bat : Enemy
{
    [Header("Flying Settings")]
    [SerializeField] private float baseFlyHeight = 0f;
    [SerializeField] private float hoverRange = 1f;
    [SerializeField] private float hoverSpeed = 2f;
    [SerializeField] private float highFlyRange = 2f;
    [SerializeField] private float highFlySpeed = 1f; 

    [SerializeField] private GameObject projectileObject;
    [SerializeField] private Transform shootPoint;

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
        float height = baseFlyHeight;
        height += Mathf.Sin(Time.time * hoverSpeed) * hoverRange;

        height += Mathf.Sin(Time.time * highFlySpeed) * highFlyRange;
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
            while(Vector3.Distance(new Vector3(transform.position.x,0, transform.position.z), new Vector3(patrolTarget.x, 0, patrolTarget.z))
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

    public void BatAttack()
    {
        if (projectileObject == null || shootPoint == null || enemyType == null)
            return;

        // spwan projectile
        GameObject projectile = Instantiate(projectileObject, shootPoint.position, shootPoint.rotation);

        // make sure projectile fires in Player direction
        Vector3 target = playerTransform.position;
        // adjust height so projeticle doesnt shoot at players feet
        target.y += 1f;
        Vector3 direction = (target - shootPoint.position).normalized;
        projectile.transform.forward = direction;

        EnemyProjectile proj = projectile.GetComponent<EnemyProjectile>();
        if (proj != null)
            proj.Init(this);
    }

    public void VariantEffects(PlayerActions player)
    {
        switch(enemyType.variant)
        {
            case Variant.One:
                player.BurnedStatus(4f);
                break;
            case Variant.Two:
                player.SlowedForward(8f);
                break;
            case Variant.Three:
                player.ParalyzedForward(6.2f);
                break;
        }
    }
}
