using System.Collections;
using UnityEngine;

public class Goblin : Enemy
{
    [Header("Player Reference")]
    [SerializeField] private LayerMask playerMask;

    [Header("Projectile")]
    [SerializeField] private GameObject projectileObject;
    [SerializeField] private Transform shootPoint;

    public void Attack()
    {
        switch(Type.variant)
        {
            case Variant.None:
                StartCoroutine(MeeleAttack());
                break;
            case Variant.One:
                WarriorAttack();
                break;
            case Variant.Two:
                ArcherAttack();
                break;
        }
    }

    private IEnumerator MeeleAttack()
    {
        Vector3 center = transform.position + transform.forward * Type.attackRange;

        Collider[] hitCollider = Physics.OverlapSphere(center, Type.attackRange, playerMask);
        foreach (Collider collider in hitCollider)
        {
            PlayerActions playerActions = collider.GetComponent<PlayerActions>();
            if (playerActions != null)
            {
                playerActions.OnTakeDamage(Type.damage);
            }
        }
        // second attack instance if still in range 
        yield return new WaitForSeconds(2.5f);

        center = transform.position + transform.forward * Type.attackRange;

        Collider[] secondHit = Physics.OverlapSphere(center, Type.attackRange, playerMask);
        foreach (Collider collider in secondHit)
        {
            PlayerActions playerActions = collider.GetComponent<PlayerActions>();
            if (playerActions != null)
            {
                playerActions.OnTakeDamage(Type.damage);
                playerActions.ApplyKnockback(transform.position, Type.knockback);
            }
        }
        enemyAnimation.SetAnimationState(EnemyAnimationState.Idle);
    }

    private void WarriorAttack()
    {
        Vector3 center = transform.position + transform.forward * Type.attackRange;

        Collider[] hitCollider = Physics.OverlapSphere(center, Type.attackRange, playerMask);
        foreach (Collider collider in hitCollider)
        {
            PlayerActions player = collider.GetComponent<PlayerActions>();
            if (player != null)
                player.OnTakeDamage(Type.damage);
        }
    }

    private void ArcherAttack()
    {
        if (projectileObject == null || shootPoint == null || Type == null)
            return;

        // spawn projectile 
        GameObject projectile = Instantiate(projectileObject, shootPoint.position, shootPoint.rotation);

        // fire in player direction and adjust upwards so projectile doesnt shoot at players feet
        Vector3 target = playerTransform.position;
        target.y += 1f;

        Vector3 direction = (target - shootPoint.position).normalized;
        projectile.transform.forward = direction;

        EnemyProjectile enemyProjectile = projectile.GetComponent<EnemyProjectile>();
        if (enemyProjectile != null) 
            enemyProjectile.Init(this);
        
        enemyAnimation.SetAnimationState(EnemyAnimationState.Idle);
    }

}
