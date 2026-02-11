using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float projectileSpeed = 1f;
    private float lifetime;
    private Bat sourceBat;
    private Vector3 startPos;



    public void Init(Bat bat)
    {
        sourceBat = bat;
        lifetime = bat.enemyType.attackRange + 1.5f;
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
        if(sourceBat == null)
        {
            Destroy(gameObject);
            return; 
        }

        PlayerActions player = other.GetComponent<PlayerActions>();
        if (player != null && player.IsAlive)
        {
            player.OnTakeDamage(sourceBat.enemyType.damage);
            Destroy(gameObject);
        }
    }

}
