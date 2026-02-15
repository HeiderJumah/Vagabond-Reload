using System.Collections;
using UnityEngine;

public class Skeleton : Enemy
{
    [Header("Gizmo Draw Debug")]
    private int currentAttack = -1; // store attack used (-1 == no attack) 

    [Header("Ranged Skeleton")]
    [SerializeField] private GameObject projectileObject;
    [SerializeField] private Transform shootPoint;


    [Header("Player Detection")]
    [SerializeField] private LayerMask playerMask;

    public void Attack()
    {
        switch (Type.variant)
        {
            case Variant.None:
                StartCoroutine(RangedSkeletonAttack());
                break;

            case Variant.One:
                StartCoroutine(SwordSkeletonAttack());
                break;
        }
    }

    private IEnumerator RangedSkeletonAttack()
    {
        yield return new WaitForSeconds(enemyType.attackWindup);

        ThrowBone();
    }


    private void ThrowBone()
    {
        if(projectileObject == null || shootPoint == null)
            return;

        Vector3 target = playerTransform.position;
        target.y = 0f;

        GameObject bone = Instantiate(projectileObject, shootPoint.position, Quaternion.identity);

        BoneProjectile projectile = bone.GetComponent<BoneProjectile>();
        if (projectile != null)
        {
            projectile.Init(this, target);
        }
    }

    private IEnumerator SwordSkeletonAttack()
    {

        currentAttack = Random.Range(0, 2);
        switch(currentAttack)
        {
            case 0:
                enemyAnimation.SetAnimationState(EnemyAnimationState.AttackOne);

                yield return new WaitForSeconds(enemyType.attackWindup);

                SwordSlash();
                break;
            case 1:
                enemyAnimation.SetAnimationState(EnemyAnimationState.AttackTwo);

                yield return new WaitForSeconds(enemyType.attackWindup);

                SwordStab();
                break;
        }

        currentAttack = -1; // reset after attack

    }

    private void SwordSlash()
    {
        Vector3 center = transform.position + transform.forward * Type.attackRange;

        Collider[] hitCollider = Physics.OverlapSphere(center, Type.attackRange, playerMask);
        foreach (Collider collider in hitCollider)
        {
            Debug.Log("Hit collider: " + collider.name);
            PlayerActions player = collider.GetComponent<PlayerActions>();
            if (player != null)
            {
                player.OnTakeDamage(Type.damage);

            }

        }
    }

    private void SwordStab()
    {
        Vector3 origin = transform.position + transform.forward * 0.3f + Vector3.up * 0.5f;
        // BoxCast size
        Vector3 BoxExtent = new Vector3(0.3f, 0.3f, 0.2f);
        Vector3 direction = transform.forward;

        RaycastHit hit;

        if (Physics.BoxCast(origin, BoxExtent, direction, out hit, transform.rotation, Type.attackRange, playerMask))
        {
            PlayerActions player = hit.collider.GetComponent<PlayerActions>();

            if (player != null)
            {
                player.OnTakeDamage(Type.damage);
            }
        }
    }


    /* private void OnDrawGizmosSelected()
     {
         if (Type == null)
             return;

         Vector3 center = transform.position + transform.forward * Type.attackRange;

         // Draw a semi-transparent red sphere
         Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
         Gizmos.DrawSphere(center, Type.attackRange);
     } */

    // let chatgpt write a quick debug draw for both attack cases

    private void OnDrawGizmosSelected()
    {
        if (Type == null)
            return;

        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);

        if (currentAttack == 0) // Slash (sphere)
        {
            Vector3 center = transform.position + transform.forward * Type.attackRange;
            Gizmos.DrawSphere(center, Type.attackRange);
        }
        else if (currentAttack == 1) // Stab (boxcast)
        {
            Vector3 origin = transform.position
                           + transform.forward * 0.3f
                           + Vector3.up * 0.5f;

            Vector3 halfExtents = new Vector3(0.3f, 0.3f, 0.2f);
            float distance = Type.attackRange;

            Matrix4x4 matrix = Matrix4x4.TRS(
                origin + transform.forward * distance * 0.5f,
                transform.rotation,
                new Vector3(halfExtents.x * 2f,
                            halfExtents.y * 2f,
                            halfExtents.z * 2f + distance)
            );

            Gizmos.matrix = matrix;
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }


}
