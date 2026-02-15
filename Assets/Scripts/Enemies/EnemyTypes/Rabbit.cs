using UnityEngine;

public class Rabbit : Enemy
{
    [SerializeField] private LayerMask playerMask;
    public void Attack()
    {
        Vector3 origin = transform.position + transform.forward * 0.3f + Vector3.up * 0.5f;
        // half the size 
        Vector3 BoxExtent = new Vector3(0.5f, 0.5f, 0.2f);
        Vector3 direction = transform.forward;

        RaycastHit hit;

        if (Physics.BoxCast(origin, BoxExtent, direction, out hit, transform.rotation, Type.attackRange, playerMask))
        {
            PlayerActions player = hit.collider.GetComponent<PlayerActions>();

            if (player != null) 
            {
                player.OnTakeDamage(Type.damage);
                VariantEffect();
            }
        }
    }

    private void VariantEffect()
    {
        switch (Type.variant)
        {
            case Variant.One:
                LifeSteal(Type.damage / 2);
                break;
            case Variant.Two:
                LifeSteal(Type.damage);
                break;
            case Variant.Three:
                LifeSteal(Type.damage * 2);
                break;
        }
    }

    private void LifeSteal(float amount)
    {
        Heal(amount);
    }

    private void OnDrawGizmosSelected()
    {
        if (Type == null)
            return;

        Vector3 origin = transform.position
                       + transform.forward * 0.3f
                       + Vector3.up * 0.5f;

        Vector3 halfExtents = new Vector3(0.5f, 0.5f, 0.2f);
        float distance = Type.attackRange;

        Vector3 endPosition = origin + transform.forward * distance;

        // Draw the full swept volume
        Gizmos.color = new Color(1f, 0f, 0f, 0.15f);

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
