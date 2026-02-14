using UnityEngine;

public class Ghost : Enemy
{
    [Header("References")]
    [SerializeField] private LayerMask playerMask;

    public void Attack()
    {
        Vector3 center = transform.position + transform.forward * Type.attackRange;

        Collider[] hitCollider = Physics.OverlapSphere(center, Type.attackRange, playerMask);

        foreach (Collider col in hitCollider)
        {
            PlayerActions player = col.GetComponent<PlayerActions>();
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
                break;
            case Variant.Two:
                break;
            case Variant.Three:
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
