using System.Collections;
using UnityEngine;

public class BoneProjectile : MonoBehaviour
{
    [Header("Throw Arc Settings")]
    [SerializeField] private float flightDuration = 1.4f;
    [SerializeField] private float arcHeight = 2f;
    [SerializeField] private float aoeRadius = 2f;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private GameObject aoeDecal;
    [SerializeField] private GameObject aoeTimerDecal;

    [Header("References")]
    private Enemy sourceEnemy;
    private Vector3 startPos;
    private Vector3 endPos;
    private float timer;
    private bool hasHit = false;
    private GameObject activeDecal;
    private GameObject activeTimerDecal;

    public void Init(Enemy enemy, Vector3 target)
    {
        // get positions 
        sourceEnemy = enemy;
        startPos = transform.position;
        endPos = target;

        // spawn aoe indicator, slightly transparent, shows full aoe attack area
        if(aoeDecal != null )
        {
            Vector3 spawnPos = endPos + Vector3.up * 0.9f;
            Quaternion spawnRotation = Quaternion.Euler(90f, 0f, 0f);
            activeDecal = Instantiate(aoeDecal, spawnPos, spawnRotation);
            activeDecal.transform.localScale = new Vector3(aoeRadius * 2f, aoeRadius * 2f, 1f);
        }
        // spawn aoe timing indicator, better visible second circle that expands to the full size over time, showing the exact moment of aoe damage
        if(aoeTimerDecal != null )
        {
            Vector3 spawnPos = endPos + Vector3.up * 0.9f;
            Quaternion spawnRotation = Quaternion.Euler(90f, 0f, 0f);
            activeTimerDecal = Instantiate(aoeTimerDecal, spawnPos, spawnRotation);

            // start small, expand over time 
            activeTimerDecal.transform.localScale = new Vector3(0.1f,0.1f, 1f);
            StartCoroutine(ScaleOverTime(activeTimerDecal, flightDuration));
        }


    }

    private void Update()
    {
       if(hasHit) 
            return;

        timer += Time.deltaTime;
        float t = timer / flightDuration;

        if (t >= 1)
        {
            Land();
            return;
        }

        // throwing arc 
        Vector3 horizontal = Vector3.Lerp(startPos, endPos, t);
        float height = Mathf.Sin(t * Mathf.PI) * arcHeight;
        transform.position = horizontal + Vector3.up * height;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(hasHit) 
            return;

        PlayerActions player = other.GetComponent<PlayerActions>();
        if(player != null)
        {
            player.OnTakeDamage(sourceEnemy.Type.damage);
            Cleanup();
        }
    }

    private void Land()
    {
        if (hasHit)
            return;

        // Aoe damage in case projectile hits its endPos
        Collider[] hit = Physics.OverlapSphere (endPos, aoeRadius, playerMask);
        foreach(Collider c in hit)
        {
            PlayerActions player = c.GetComponent<PlayerActions>();
            if(player != null)
            {
                float damageToApply = sourceEnemy.Type.damage * sourceEnemy.Type.attackMulitplier;

                player.OnTakeDamage (damageToApply);
            }
        }
        Cleanup();
    }

    private IEnumerator ScaleOverTime(GameObject decal, float duration)
    {
        float timer = 0f;
        Vector3 fullScale = new Vector3(aoeRadius * 2f, aoeRadius * 2f, 1f);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            decal.transform.localScale = Vector3.Lerp(new Vector3(0.1f, 0.1f, 1f), fullScale, t);

            yield return null;
        }

        decal.transform.localScale = fullScale;
    }

    private void Cleanup()
    {
        hasHit = true;

        if(activeDecal != null)
            Destroy(activeDecal);

        if(activeTimerDecal != null)
            Destroy(activeTimerDecal);

        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if(!Application.isPlaying) return;
        if(hasHit) return;

        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawSphere(endPos, aoeRadius);
    }
#endif
}
