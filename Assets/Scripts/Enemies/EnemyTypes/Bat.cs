using UnityEngine;

public class Bat : MonoBehaviour
{   
    public EnemyTypes enemyType;
    [SerializeField] private GameObject projectileObject;
    [SerializeField] private Transform shootPoint;
    public void Attack()
    {
        if (projectileObject == null || shootPoint == null || enemyType == null)
            return;

        // spwan projectile
        GameObject projectile = Instantiate(projectileObject, shootPoint.position, shootPoint.rotation);

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
