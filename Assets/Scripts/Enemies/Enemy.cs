using NUnit.Framework.Internal.Filters;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{

    [Header("Health")]
    private float currentHealth;
    private float maxHealth;
    private float healthFill;
    [SerializeField] private float healthLerpSpeed =3f;
    [SerializeField] private GameObject healthCanvas;
    [SerializeField] private Image healthbar;

    [Header("HitIndicator")]
    private Renderer[] enemyRenderers;
    [SerializeField] private Color hitColor = Color.red; 
    [SerializeField] private float hitIndicatorDuration = 1f;

    [Header("Patrolling")]
    private Vector3 patrolTarget;

    [Header("References")]
    [SerializeField] private EnemyTypes enemyType;
    private Transform playerTransform;
    private PlayerActions playerActions;
    private Vector3 originalPosition;
    private EnemyAnimation enemyAnimation;
    private Coroutine attackRoutine;
    private Coroutine smoothHealthRoutine;
    private Color[] originalColors;
    private Coroutine hitIndicatorRoutine;
    private Coroutine patrolRoutine;

    [Header("bools")]
    private bool isDead = false;
    private bool canAttack = true;
    private bool canMove = true;
    private bool showHealthbar = false;
    private bool isPatrolling = false;

    [Header("Obstacle Detection")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float avoidDistance = 1.5f;
    [SerializeField] private float rayAngle = 45f;

    private void Awake()
    {
        enemyAnimation = GetComponent<EnemyAnimation>();
        maxHealth = enemyType.health;
        currentHealth = maxHealth;

        enemyRenderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[enemyRenderers.Length];

        for (int i = 0; i < enemyRenderers.Length; i++)
        {
            originalColors[i] = enemyRenderers[i].material.color;
        }

    }


    public void TakeDamage(float damage)
    {
        if(isDead)
            return;

        currentHealth -= damage;
        // clamp the health between 0 and 1 (percentage)
        float healthPercentage = Mathf.Clamp01(currentHealth / maxHealth);
        healthFill = healthPercentage;

        // show healthbar only when enemy took damage
        if(!showHealthbar && healthPercentage < 1f)
        {
            showHealthbar = true;
            healthCanvas.SetActive(true);
        }

        //coroutine for smoother healthbar changes 
        // saftey in case damage happens to fast 
        if(smoothHealthRoutine  != null) 
            StopCoroutine(smoothHealthRoutine);
        smoothHealthRoutine = StartCoroutine(SmoothHealthChange());

        // start hit indicator routine 
        if(hitIndicatorRoutine != null)
            StopCoroutine(hitIndicatorRoutine);
        hitIndicatorRoutine = StartCoroutine(HitIndicator());

        Debug.Log("Enemy has: " +currentHealth);
        Debug.Log($"Enemy Hp: {healthPercentage * 100f}%");
        Debug.Log($"Taking damage: {damage}, currentHealth before: {currentHealth}");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    private IEnumerator HitIndicator()
    {
        if (enemyRenderers == null)
            yield break;
        // hit indicator flash
        Color hitColor = Color.white;  

        foreach(var renderers in enemyRenderers)
        {
            // make sure material allows emisiion 
            if (renderers.material.HasProperty("_EmissionColor"))
            {
                // if so make sure emission is enabled
                renderers.material.EnableKeyword("_EMSSION");
                // set emission color to hitColor for visibility 
                renderers.material.SetColor("_EmissionColor", hitColor);
            }
            else
            {
                renderers.material.color = hitColor;
            }
        }

        yield return new WaitForSeconds(hitIndicatorDuration);
        // restore original color
        for(int i = 0;i < enemyRenderers.Length;i++)
        {
            enemyRenderers[i].material.color = originalColors[i];
            // reset emission
            if (enemyRenderers[i].material.HasProperty("_EmissionColor"))
            {
                enemyRenderers[i].material.SetColor("_EmissionColor", Color.black);
            }
        }
    }

    private IEnumerator SmoothHealthChange()
    {
        while(!Mathf.Approximately(healthbar.fillAmount, healthFill))
        {
            healthbar.fillAmount = Mathf.Lerp(healthbar.fillAmount, healthFill, Time.deltaTime * healthLerpSpeed);

            yield return null;
        }

        healthbar.fillAmount = healthFill; 
    }

    public void ApplyKnockback(Vector3 source, float force)
    {
        if(isDead) 
            return;

        // get direction of knockback
        Vector3 knockbackDirection = transform.position - source;
        knockbackDirection.Normalize();
        knockbackDirection.y = 0f;

        // account enemies knockback resistance 
        float forceAdjustment = force / enemyType.hitKnockbackResistance;
        Vector3 targetPostion = transform.position + knockbackDirection * forceAdjustment;

        // Apply smooth knockback  
        StartCoroutine(SmoothKnockback(targetPostion, 0.2f));
    }

    private IEnumerator SmoothKnockback(Vector3 targetPosition, float duration)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while(elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;
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
            if (!isPatrolling)
            {
                ReturnToSpawn();
                Debug.Log("returning");
            }
        }
    }

    private void StartPatrolling()
    {
        if (isPatrolling || !canMove)
            return;

        Debug.Log("StartingPatrol");
        isPatrolling = true;
        patrolRoutine = StartCoroutine(PatrolRoutine());
    }

    private IEnumerator PatrolRoutine()
    {
        while (isPatrolling && !isDead)
        {
            // choose random point around its spawn point in patrolling range to move to
            Vector2 randomCircle = Random.insideUnitCircle * enemyType.patrollingRange;
            patrolTarget = originalPosition + new Vector3(randomCircle.x, 0, randomCircle.y);

            // fallback if random point is unreachable
            float patrolTimer = 0f;

            // wait short time before moving to new position 
            while (Vector3.Distance(transform.position, patrolTarget) > 0.1f && patrolTimer < 5f && !isDead)
            {
                Vector3 direction = GetMoveDirection(patrolTarget);
                transform.position += direction * enemyType.speed * Time.deltaTime;

                enemyAnimation.SetAnimationState(EnemyAnimationState.Move);
                patrolTimer += Time.deltaTime;

                yield return null;
            }

            // wait a short time after reaching patrol point before patrolling again
            enemyAnimation.SetAnimationState(EnemyAnimationState.Idle);
            yield return new WaitForSeconds(enemyType.patrolWaitTime);
        }
    }

    private void ChasePlayer()
    {
        if (!canMove)
            return;

        // when Player spotted, chase player and stop patrolling
        if(isPatrolling)
        {
            StopCoroutine(patrolRoutine);
            isPatrolling= false;
        }

        enemyAnimation.SetAnimationState(EnemyAnimationState.Move);

        Vector3 direction = GetMoveDirection(playerTransform.position);
        transform.position += direction * enemyType.speed * Time.deltaTime;
    }

    private void Attack()
    {
        if(!canAttack)
            return;

        // stop patrolling when trying to attack player
        if(isPatrolling && patrolRoutine != null)
        {
            StopCoroutine(patrolRoutine);
            isPatrolling= false;
        }

        canAttack = false;
        canMove = false;
        
        enemyAnimation.SetAnimationState(EnemyAnimationState.AttackOne);

        if(attackRoutine != null)
            StopCoroutine(attackRoutine);

        attackRoutine = StartCoroutine(AttackRoutine());
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


    private void ReturnToSpawn()
    {
        float distanceToSpawn = Vector3.Distance(transform.position, originalPosition);

        if (distanceToSpawn > 0.1f)
        {
            if (!canMove)
                return;

            enemyAnimation.SetAnimationState(EnemyAnimationState.Move);

            Vector3 direction = GetMoveDirection(originalPosition);
            transform.position += direction * enemyType.speed * Time.deltaTime;
           // transform.position = Vector3.MoveTowards(transform.position, originalPosition, enemyType.speed * Time.deltaTime);
        }
        else
        {
            // Go Idle when at spawn
            enemyAnimation.SetAnimationState(EnemyAnimationState.Idle);
    
        }
    }

    private void RotateTowordsTarget()
    {
        Vector3 moveDirection = Vector3.zero;

        if (playerTransform != null && playerActions.IsAlive)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

           if (distanceToPlayer <= enemyType.targetingRange)
            {
                moveDirection = playerTransform.position - transform.position;
            }
        }

        if (moveDirection == Vector3.zero && !isPatrolling)
        {
           moveDirection = originalPosition - transform.position;
        }
        if (moveDirection == Vector3.zero && isPatrolling)
        {
           moveDirection = patrolTarget - transform.position;
        }

        moveDirection.y = 0f;

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion rotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 10f);
        }

    }

    private Vector3 GetMoveDirection(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;

        Vector3 origin = transform.position + Vector3.up * 0.5f;

        // shoot ray forward
        bool forwardRay = Physics.Raycast(origin, direction, avoidDistance, obstacleLayer);

        //side rays (left and right) 
        Vector3 leftDirection = Quaternion.Euler(0, -rayAngle, 0) * direction;
        Vector3 rightDirection = Quaternion.Euler(0, rayAngle, 0) * direction;

        bool leftRay = Physics.Raycast(origin, leftDirection, avoidDistance, obstacleLayer);
        bool rightRay = Physics.Raycast(origin, rightDirection, avoidDistance, obstacleLayer);


        // adjust direction based on raycasts detection
        if (forwardRay)
        {
            if (!rightRay)
                direction = rightDirection;
            else if (!leftRay)
                direction = leftDirection;
            else
                // fallback if stuck
                direction = -direction;
        }

        direction.y = 0f;
        direction.Normalize();

        return direction;

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

        // start patrolling when Idle and not fighting / chasing player
        if(!isPatrolling && canMove && playerTransform != null && Vector3.Distance(transform.position, originalPosition) <= 0.5f 
            && Vector3.Distance(transform.position, playerTransform.position) > enemyType.targetingRange)
                StartPatrolling();
    }

}

