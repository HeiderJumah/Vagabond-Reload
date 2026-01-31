using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{

    [Header("Stats")]
    private float currentHealth;

    [Header("References")]
    [SerializeField] private EnemyTypes enemyType;
   // private NavMeshAgent agent;
    private Transform playerTransform;
    private Vector3 originalPosition;
    private EnemyAnimation enemyAnimation;

    [Header("bools")]
    private bool isDead = false;
    private bool canAttack = true;
    private bool canMove = true;

    private void Awake()
    {
        //agent = GetComponent<NavMeshAgent>();
        enemyAnimation = GetComponent<EnemyAnimation>();

        currentHealth = enemyType.health;
        //agent.speed = enemyType.speed;
        //agent.stoppingDistance = enemyType.attackRange;
    }

#region Enemy Targeting Behavior

    /// <summary>
    /// Find the player and set as destination
    /// </summary>
  /*  private void TargetPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= enemyType.targetingRange)
        {
            if(!canMove)
                return;
            if(distanceToPlayer > enemyType.attackRange)
            {
                // Set the player's position as the destination for the NavMeshAgent
                agent.isStopped = false;
                agent.SetDestination(playerTransform.position);
                enemyAnimation.SetAnimationState(EnemyAnimationState.Move);
            }
            else
            {
                // Attack the player
                agent.isStopped = true;
                enemyAnimation.SetAnimationState(EnemyAnimationState.AttackOne);
            }
        }
        else
        {
            // go back to idle state and original position
            if (Vector3.Distance(transform.position, originalPosition) > 0.1f)
            {
                agent.isStopped = false;
                agent.SetDestination(originalPosition);
                enemyAnimation.SetAnimationState(EnemyAnimationState.Move);
            }
            else
            {
                agent.isStopped = true;
                enemyAnimation.SetAnimationState(EnemyAnimationState.Idle);

            }
        }
    }


    private void Rotate()
    {
        Vector3 velocity = agent.velocity;

        velocity.y = 0; // Keep only the horizontal direction

        if (velocity != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }
    }*/

#endregion

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        Debug.Log(currentHealth);
        Debug.Log($"Taking damage: {damage}, currentHealth before: {currentHealth}");
        //float healthPercentage = Mathf.Clamp01(currentHealth / enemyType.health);

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        Debug.Log("Enemy died");
        enemyAnimation.SetAnimationState(EnemyAnimationState.Death);
    }

    private void OnDeathEvent()
    {
        GameObject.Destroy(this);
    }
    void Start()
    {
        // store the initial position
        originalPosition = transform.position; 

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

    }

    void Update()
    {
        if (isDead || playerTransform == null)
        {
            return;
        }
       // TargetPlayer();
        //Rotate();
    }
}
