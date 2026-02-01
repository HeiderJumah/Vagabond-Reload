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
    private PlayerActions playerActions;
    private Vector3 originalPosition;
    private EnemyAnimation enemyAnimation;
    private Coroutine attackRoutine;

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

        Debug.Log("Enemy has: " +currentHealth);
        Debug.Log($"Taking damage: {damage}, currentHealth before: {currentHealth}");
        //float healthPercentage = Mathf.Clamp01(currentHealth / enemyType.health);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        canAttack = false;
        canMove = false;   

        Debug.Log("Enemy died");
        enemyAnimation.SetAnimationState(EnemyAnimationState.Death);
    }

    /// <summary>
    ///  animation event, destory on last frame of death animation
    /// </summary>
    private void OnDeathEvent()
    {
        GameObject.Destroy(gameObject);
    }
    /// <summary>
    /// animation event, set enemy back to idle on last frame of attack
    /// </summary>
    private void OnAttackEnded()
    {
        enemyAnimation.SetAnimationState(EnemyAnimationState.Idle);
    }
    void Start()
    {
        // store the initial position
        originalPosition = transform.position; 

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerActions = player.GetComponent<PlayerActions>();
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
                if(canAttack)
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

        if(attackRoutine != null)
            StopCoroutine(attackRoutine);

        attackRoutine = StartCoroutine(AttackRoutine());
       // StartCoroutine(ResetAttack(enemyType.attackCooldown));
    }

    private IEnumerator AttackRoutine()
    { 
        // windup time so the player has a chance to evade
        yield return  new WaitForSeconds(enemyType.attackWindup);

        if(playerActions == null || !playerActions.IsAlive)
            yield break;

        switch(enemyType.enemyCategory)
        {
            case EnemyCategory.slime:
                Slime slime = GetComponent<Slime>();
                if (slime != null)
                    slime.Attack();
                break;
            case EnemyCategory.skeleton:
                break;
            case EnemyCategory.swordSkeleton:
                break;
            case EnemyCategory.bat:
                break;
            case EnemyCategory.goblin:
                break;
            case EnemyCategory.rabbit:
                break;
            case EnemyCategory.golemBoss:
                break;
        }

        // recovery time after attack 

        yield return new WaitForSeconds(enemyType.attackCooldown);

        canAttack = true;
        Debug.Log("Enemy canAttack: " + canAttack);
        canMove= true;
        Debug.Log("Enemy canMove: " + canMove);

    }

    /*private IEnumerator ResetAttack(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        canAttack = true;
        Debug.Log(canAttack);
        canMove = true;
    }*/


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
        if (!playerActions.IsAlive)
        {
            ReturnToSpawn();
            return; 
        }

        HandleEnemyBehavior();
        RotateTowordsTarget();
    }

}

