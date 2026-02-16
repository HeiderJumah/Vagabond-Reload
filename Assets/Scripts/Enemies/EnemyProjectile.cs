using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float projectileSpeed = 1f;
    private float lifetime;
    //private Bat sourceBat;
    private Enemy sourceEnemy;
    private Vector3 startPos;



    public void Init(Enemy enemy)
    {
        sourceEnemy = enemy;
        lifetime = enemy.Type.attackRange + 1.5f;
        startPos = transform.position;
    }

    private void Update()
    {
        transform.position += transform.forward * projectileSpeed * Time.deltaTime;

        // destory if max distance reached 
        if(Vector3.Distance(startPos, transform.position) >= lifetime)
            Destroy(gameObject);
    }
    private void OnTriggerEnter(Collider other)
    {
        if(sourceEnemy == null)
        {
            Destroy(gameObject);
            return; 
        }

        PlayerActions player = other.GetComponent<PlayerActions>();
        if (player != null && player.IsAlive)
        {
            float damageToApply = sourceEnemy.Type.damage * sourceEnemy.Type.attackMulitplier;

            player.OnTakeDamage(damageToApply);
            Bat bat = sourceEnemy as Bat;
            if (bat != null)
            {
                bat.VariantEffects(player);
            }
            Destroy(gameObject);
        }
    }

}
