using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{

    [Header("Stats")]
    private float health;

    [Header("References")]
    [SerializeField] private EnemyTypes enemyType;
    private NavMeshAgent agent;
    private Transform playerTransform;

    [Header("bools")]
    private bool isDead = false;
    private bool canAttack = true;
    private bool canMove = true;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        
        health = enemyType.health;
        agent.speed = enemyType.speed;
        agent.stoppingDistance = enemyType.attackRange;
    }

#region Enemy Targeting Behavior

    /// <summary>
    /// Find the player and set as destination
    /// </summary>
    private void TargetPlayer()
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
            }
            else
            {
                // Attack the player
                agent.isStopped = true;
            }
        }
        else
        {
            // go back to idle state and original position
            agent.isStopped = true;
        }
    }

    private void RotateTowardsPlayer()
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0; // Keep only the horizontal direction

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }
    }

#endregion

    void Start()
    {
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
        TargetPlayer();
        RotateTowardsPlayer();
    }
}
