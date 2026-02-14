using UnityEngine;

public class Slime : Enemy
{
    [Header("References")]
    //[SerializeField] EnemyTypes enemyTypes;
    [SerializeField] private LayerMask playerMask;

    /// <summary>
    /// For now simple attack logic, can be expended later with different slime EnemyTypes
    /// </summary>
    public void Attack()
    {
        Vector3 center = transform.position + transform.forward * Type.attackRange;

        Collider[] hitCollider = Physics.OverlapSphere(center, Type.attackRange, playerMask);
        Debug.Log($"Hit colliders count: {hitCollider.Length}");
        foreach (Collider col in hitCollider)
        {
            Debug.Log("Hit collider: " + col.name);
            PlayerActions player = col.GetComponent<PlayerActions>();
            if (player == null)
            {
                player = col.GetComponentInParent<PlayerActions>();
                Debug.LogWarning("No Player component found for collider: " + col.name);
            }
            if (player != null)
            {
                player.OnTakeDamage(Type.damage);
                VariantEffects(player);
            }
        }
    }

    private void VariantEffects(PlayerActions player)
    {
        switch(Type.variant)
        {
            case Variant.One:
                player.BurnedStatus(5f);
                break;
            case Variant.Two:
                player.SlowedForward(5f);
                break;
            case Variant.Three:
                player.ParalyzedForward(6.2f);
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
