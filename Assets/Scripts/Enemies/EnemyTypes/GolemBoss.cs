using System.Collections;
using UnityEngine;

public class GolemBoss : Enemy
{

    [Header("Ranged Settings")]
    [SerializeField] private GameObject projectileObject;
    [SerializeField] private GameObject LineThrowObject;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float rangedOneAdjustTime;
    private int currentRangedAttack;

    [Header("Close Range Settings")]
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private GameObject aoeDecal;
    [SerializeField] private GameObject aoeTimerDecal;
    [SerializeField] private float closeRangeAdjust;
    protected override void HandleEnemyBehavior()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);


        if (distanceToPlayer > enemyType.targetingRange)
        {
            if (!isPatrolling)
            {
                ReturnToSpawn();
            }
            return;
        }

        // close range attack
        if (distanceToPlayer <= enemyType.attackCloseRange)
        {
            if (canAttack)
            {
                Attack(AttackType.Close);
            }
            return;
        }
        // ranged attack 
        if (distanceToPlayer <= enemyType.attackRange)
        {
            if (canAttack)
            {
                Attack(AttackType.Ranged);
            }
            return;
        }
        // chase player
        ChasePlayer();
    }

    private void Attack(AttackType type)
    {
        if (!canAttack)
            return;

        canAttack = false;
        canMove = false;

        switch (type)
        {
            case AttackType.Close:
                if(!isDead) 
                    StartCoroutine(CloseAttack());
                break;

            case AttackType.Ranged:
                if (!isDead)
                {
                    currentRangedAttack = Random.Range(0, 2);
                    ChooseRangedAttack();
                }
                break;
        }
    }

    private IEnumerator CloseAttack()
    {
        // aoe around Golem in circle
        enemyAnimation.SetAnimationState(EnemyAnimationState.AttackTwo);


        GameObject activeDecal = null;
        GameObject activeTimerDecal = null;

        if(aoeDecal != null)
        {
            // spawn at the center of the golem
            Vector3 spawnPos = transform.position + Vector3.up * 1f; 
            Quaternion spawnRot = Quaternion.Euler(90f,0f,0f);
            activeDecal = Instantiate(aoeDecal, spawnPos, spawnRot);
            activeDecal.transform.localScale = Vector3.one * enemyType.closeRangeAoe * 2f; // full circle around golem 
        }

        if(aoeTimerDecal != null)
        {
            // fill indicator decal 
            // spawn at the center of the golem
            Vector3 spawnPos = transform.position + Vector3.up * 1f;
            Quaternion spawnRot = Quaternion.Euler(90f, 0f, 0f);
            activeTimerDecal = Instantiate(aoeTimerDecal, spawnPos, spawnRot);
            activeTimerDecal.transform.localScale = Vector3.one * enemyType.closeRangeAoe * 0.1f; // satrt small and get bigger over time
            StartCoroutine(ScaleOverTime(activeTimerDecal, enemyType.attackWindup));
        }

        yield return new WaitForSeconds(enemyType.attackWindup * closeRangeAdjust);

        // Deal Damage
        Collider[] hitCollider = Physics.OverlapSphere(transform.position, enemyType.closeRangeAoe, playerMask);
        foreach(Collider collider in hitCollider)
        {
            PlayerActions player = collider.GetComponent<PlayerActions>();
            if(player != null)
            {
                player.OnTakeDamage(enemyType.damage);
                player.ApplyKnockback(transform.position, enemyType.knockback);
            }
        }

        // clean up the decals 
        if (activeDecal != null) 
            Destroy(activeDecal);
        if(activeTimerDecal != null)
            Destroy(activeTimerDecal);

        StartCoroutine(AttackCooldown(1f));
    }

    private IEnumerator ScaleOverTime(GameObject decal, float duration)
    {
        float timer = 0f;
        Vector3 fullyScaled = Vector3.one * enemyType.closeRangeAoe * 2f;

        while ( timer < duration )
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            decal.transform.localScale = Vector3.Lerp(Vector3.one * 0.1f, fullyScaled, t);
            yield return null;
        }
        decal.transform.localScale = fullyScaled;
    }

    private void ChooseRangedAttack()
    {
        if (currentRangedAttack == 0)
            StartCoroutine(ThrowInStraightLine());
        else
            StartCoroutine(ThrowInArc());
    }
    private IEnumerator ThrowInStraightLine()
    {
        if(LineThrowObject == null || shootPoint == null)
            yield break;

        enemyAnimation.SetAnimationState(EnemyAnimationState.AttackThree);

        yield return new WaitForSeconds(enemyType.attackWindup + rangedOneAdjustTime);

        GameObject projectile = Instantiate(LineThrowObject, shootPoint.position, shootPoint.rotation);

        Vector3 target = playerTransform.position;
        target.y += 1f;

        Vector3 direction = (target - shootPoint.position).normalized;
        projectile.transform.forward = direction;

        EnemyProjectile proj = projectile.GetComponent<EnemyProjectile>();
        if (proj != null)
            proj.Init(this);

        StartCoroutine(AttackCooldown(2f));
    }

    private IEnumerator ThrowInArc()
    {
        if (projectileObject == null || shootPoint == null)
            yield break;

        enemyAnimation.SetAnimationState(EnemyAnimationState.AttackOne);

        yield return new WaitForSeconds(enemyType.attackWindup);

        Vector3 target = playerTransform.position;
        target.y = 0f;

        GameObject stone = Instantiate(projectileObject, shootPoint.position, Quaternion.identity);

        BoneProjectile projectile = stone.GetComponent<BoneProjectile>();
        if (projectile != null)
        {
            projectile.Init(this, target);
        }
        StartCoroutine(AttackCooldown(1f));

    }

    private IEnumerator AttackCooldown(float adjust)
    {

        enemyAnimation.SetAnimationState(EnemyAnimationState.Idle);
        Debug.Log("Idle");
        yield return new WaitForSeconds(enemyType.attackCooldown + adjust);

        canAttack = true;
        canMove= true;

    }


    private enum AttackType
    {
        Close,
        Ranged
    }

}
