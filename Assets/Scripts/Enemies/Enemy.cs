using NUnit.Framework.Internal.Filters;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{

    [Header("Stats")]
    private float currentHealth;

    [Header("References")]
    [SerializeField] private EnemyTypes enemyType;
    private Transform playerTransform;
    private Vector3 originalPosition;
    private EnemyAnimation enemyAnimation;

    [Header("bools")]
    private bool isDead = false;
    private bool canAttack = true;
    private bool canMove = true;

    [Header("Obstacle Detection")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float avoidDistance = 1.5f;
    [SerializeField] private float rayAngle = 30f;

    private void Awake()
    {
        enemyAnimation = GetComponent<EnemyAnimation>();
        currentHealth = enemyType.health;
    }


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
        isDead = true;
        canAttack = false;
        canMove = false;   

        Debug.Log("Enemy died");
        enemyAnimation.SetAnimationState(EnemyAnimationState.Death);
    }

    private void OnDeathEvent()
    {
        GameObject.Destroy(gameObject);
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

    private void HandleEnemyBehavior()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Player is in targeting range 
        if (distanceToPlayer <= enemyType.targetingRange)
        {
            if (distanceToPlayer <= enemyType.attackRange)
            {
                Attack();
            }
            else
            {
                ChasePlayer();
            }
        }
        else
        {
            ReturnToSpawn();
        }
    }

    private void ChasePlayer()
    {
        if (!canMove)
            return;

        enemyAnimation.SetAnimationState(EnemyAnimationState.Move);

        Vector3 direction = (playerTransform.position - transform.position).normalized;

        Vector3 origin = transform.position + Vector3.up * 0.5f;

        // shoot ray forward
        bool forwardRay = Physics.Raycast(origin, direction, avoidDistance, obstacleLayer);

        //side rays (left and right) 
        Vector3 leftDirection = Quaternion.Euler(0, -rayAngle, 0) * direction;
        Vector3 rightDirection = Quaternion.Euler(0, rayAngle, 0) * direction;

        bool leftRay = Physics.Raycast(origin, leftDirection, avoidDistance, obstacleLayer);
        bool rightRay = Physics.Raycast(origin, rightDirection, avoidDistance, obstacleLayer);

        // choose direction based on raycasts
        if (forwardRay)
        {
            if(!rightRay)
                direction = rightDirection;
            else if(!leftRay)
                direction = leftDirection;
            else
                direction = -direction;
        }

        direction.y = 0f;
        direction.Normalize();

        transform.position += direction * enemyType.speed * Time.deltaTime;
    }

    private void Attack()
    {
        if(!canAttack)
            return;

        canAttack = false;
        canMove = false;

        enemyAnimation.SetAnimationState(EnemyAnimationState.AttackOne);

        StartCoroutine(ResetAttack(enemyType.attackCooldown));
    }

    private IEnumerator ResetAttack(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        canAttack = true;
        canMove = true;
    }

    private void ReturnToSpawn()
    {
        float distanceToSpawn = Vector3.Distance(transform.position, originalPosition);

        if (distanceToSpawn > 0.1f)
        {
            if (!canMove)
                return;

            enemyAnimation.SetAnimationState(EnemyAnimationState.Move);

            transform.position = Vector3.MoveTowards(transform.position, originalPosition, enemyType.speed * Time.deltaTime);
        }
        else
        {
           enemyAnimation.SetAnimationState(EnemyAnimationState.Idle);
        }
    }

    private void RotateTowordsTarget()
    {
        Vector3 target = playerTransform.position;

        if(Vector3.Distance(transform.position, playerTransform.position) > enemyType.targetingRange)
            target = originalPosition;

        Vector3 direction = target - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.1f)
            return;

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 10f);
    }

    void Update()
    {
        if (isDead || playerTransform == null)
        {
            return;
        }
        HandleEnemyBehavior();
        RotateTowordsTarget();
    }

}

