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
}
