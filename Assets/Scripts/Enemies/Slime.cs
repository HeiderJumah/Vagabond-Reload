using UnityEngine;

public class Slime : MonoBehaviour
{
    [Header("References")]
    [SerializeField] EnemyTypes enemyTypes;
    [SerializeField] private LayerMask playerMask;

    /// <summary>
    /// For now simple attack logic, can be expended later with different slime EnemyTypes
    /// </summary>
    public void Attack()
    {
        Vector3 center = transform.position + transform.forward * enemyTypes.attackRange;

        Collider[] hitCollider = Physics.OverlapSphere(center, enemyTypes.attackRange, playerMask);
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
                player.OnTakeDamage(enemyTypes.damage);
                switch(enemyTypes.variant)
                {
                    case Variant.None:
                        Debug.Log("No Variant");
                        break;
                    case Variant.One:
                        player.BurnedStatus(5f);
                        break;
                    case Variant.Two:
                        break;
                    case Variant.Three:
                        break;
                }

            }
        }
    }

    private void VariantEffects(PlayerActions player)
    {
        switch(enemyTypes.variant)
        {
            case Variant.None:
                break;
            case Variant.One:
                break;
            case Variant.Two:
                break;
            case Variant.Three:
                break;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (enemyTypes == null)
            return;


        Vector3 center = transform.position + transform.forward * enemyTypes.attackRange;

        // Draw a semi-transparent red sphere
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawSphere(center, enemyTypes.attackRange);
    }

}
