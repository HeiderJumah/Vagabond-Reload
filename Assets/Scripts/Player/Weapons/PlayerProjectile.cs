using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    [SerializeField] private float projectileSpeed = 1f;
    private float lifetime;
    private PlayerActions source;
    private Vector3 startPos;



    public void Init(PlayerActions player)
    {
        source = player;
        lifetime = player.Stats.attackRange * player.Weapon.weaponRange;
        startPos = transform.position;
    }

    private void Update()
    {
        transform.position += transform.forward * projectileSpeed * Time.deltaTime;

        // destory if max distance reached 
        if (Vector3.Distance(startPos, transform.position) >= lifetime)
            Destroy(gameObject);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (source == null)
        {
            Destroy(gameObject);
            return;
        }

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            float damageToApply = source.Stats.damage * source.Weapon.power;

            enemy.TakeDamage(damageToApply);
            Destroy(gameObject);
        }
    }

}
