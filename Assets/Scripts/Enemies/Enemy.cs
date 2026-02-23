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
    protected Vector3 patrolTarget;

    [Header("References")]
    [SerializeField] protected EnemyTypes enemyType;
    public EnemyTypes Type => enemyType;
    protected Transform playerTransform;
    private PlayerActions playerActions;
    protected Vector3 originalPosition;
    protected EnemyAnimation enemyAnimation;
    protected Coroutine attackRoutine;
    private Coroutine smoothHealthRoutine;
    private Color[] originalColors;
    private Coroutine hitIndicatorRoutine;
    protected Coroutine patrolRoutine;
    private Vector3 deathPosition;

    [Header("bools")]
    protected bool isDead = false;
    protected bool canAttack = true;
    protected bool canMove = true;
    private bool showHealthbar = false;
    protected bool isPatrolling = false;

    [Header("Obstacle Detection")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float avoidDistance = 1.5f;
    [SerializeField] private float rayAngle = 45f;

    [SerializeField] private BarrierTrigger barrierTrigger; // Reference to the BarrierTrigger script

    private void Awake()
    {
        enemyAnimation = GetComponent<EnemyAnimation>();
        maxHealth = enemyType.health;
        currentHealth = maxHealth;

        enemyRenderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[enemyRenderers.Length];

        for (int i = 0; i < enemyRenderers.Length; i++)
        {
            if (enemyRenderers[i].material.HasProperty("_Color")) 
            {
                originalColors[i] = enemyRenderers[i].material.color;
            }
            else
            {
                // fallback 
                originalColors[i] = Color.white;
            }
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
                renderers.material.EnableKeyword("_EMISSION");
                // set emission color to hitColor for visibility 
                renderers.material.SetColor("_EmissionColor", hitColor);
            }
            else if(renderers.material.HasProperty("_Color"))
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

    protected void Heal(float amount)
    {
        if(isDead)
            return;

        if (currentHealth >= maxHealth)
            return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log(currentHealth);

        float healthPercentage = Mathf.Clamp01(currentHealth / maxHealth);
        healthFill = healthPercentage;

        if(smoothHealthRoutine != null) 
            StopCoroutine(smoothHealthRoutine);

        smoothHealthRoutine = StartCoroutine(SmoothHealthChange());
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
        
        if(attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
        if(patrolRoutine != null)
        {
            StopCoroutine(patrolRoutine);
            patrolRoutine = null;
        }

        deathPosition = transform.position;

        if(enemyType.enemyCategory == EnemyCategory.bat || enemyType.enemyCategory == EnemyCategory.ghost)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if(rb != null)
                rb.isKinematic = false;
                rb.useGravity = true;
        }

        Debug.Log("Enemy died");
        enemyAnimation.SetAnimationState(EnemyAnimationState.Death);

        if(enemyType.enemyCategory == EnemyCategory.golemBoss)
        {
            // change back to normal music when boss dies
            MusicManager.Instance.PlayMusic();

            if(barrierTrigger != null)
            {
                barrierTrigger.OnBossDeafed();
            }

        }

        StartCoroutine(DestroyObject());
    }

    private IEnumerator DestroyObject()
    {
        yield return null; // wait for death animation tp start

        // get the length of the death animation 
        float deathTime = enemyAnimation.GetAnimationTime(); 
        yield return new WaitForSeconds(deathTime);

        Instantiate(enemyType.loot, deathPosition, Quaternion.identity);

        Destroy(gameObject);
    }

    protected virtual IEnumerator AttackEnd()
    {
        yield return null ; // wait to switch states

        float attackTime = enemyAnimation.GetAnimationTime();

        // end the animation state for the bat faster (cosmetic change only)
        float exitTime = 1f;

        if (enemyType.enemyCategory == EnemyCategory.bat)
        {
            exitTime = 0.4f;
        }

        yield return new WaitForSeconds(attackTime * exitTime);

        if(isDead)
            yield break;

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

    protected virtual void HandleEnemyBehavior()
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

    protected virtual IEnumerator PatrolRoutine()
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

    protected virtual void ChasePlayer()
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

    protected virtual void Attack()
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

        if (enemyType.enemyCategory != EnemyCategory.skeleton)
        {
            enemyAnimation.SetAnimationState(EnemyAnimationState.AttackOne);
        }
        if(attackRoutine != null)
            StopCoroutine(attackRoutine);
            attackRoutine = StartCoroutine(AttackRoutine());

            StartCoroutine(AttackEnd());
    }

    private IEnumerator AttackRoutine()
    { 
        // windup time so the player has a chance to evade
        if(enemyType.enemyCategory != EnemyCategory.skeleton)
        {
            yield return  new WaitForSeconds(enemyType.attackWindup);

        }
        if(playerActions == null || !playerActions.IsAlive)
            yield break;

        switch(enemyType.enemyCategory)
        {
            case EnemyCategory.slime:
                Slime slime = GetComponent<Slime>();
                if (slime != null)
                    slime.SlimeAttack();
                break;
            case EnemyCategory.skeleton:
                Skeleton skeleton = GetComponent<Skeleton>();
                if (skeleton != null)
                    skeleton.SkeletonAttack();
                break;
            case EnemyCategory.bat:
                Bat bat = GetComponent<Bat>();
                if (bat != null)
                    bat.BatAttack();
                break;
            case EnemyCategory.goblin:
                Goblin goblin = GetComponent<Goblin>();
                if (goblin != null)
                    goblin.GoblinAttack();
                break;
            case EnemyCategory.rabbit:
                Rabbit rabbit = GetComponent<Rabbit>();
                if (rabbit != null)
                    rabbit.RabbitAttack();
                break;
            case EnemyCategory.ghost:
                Ghost ghost = GetComponent<Ghost>();
                if (ghost != null)
                    ghost.GhostAttack();
                break;
        }

        // recovery time after attack 

        yield return new WaitForSeconds(enemyType.attackCooldown);

        canAttack = true;
        Debug.Log("Enemy canAttack: " + canAttack);
        canMove= true;
        Debug.Log("Enemy canMove: " + canMove);

    }


    protected virtual void ReturnToSpawn()
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

    protected virtual Vector3 GetMoveDirection(Vector3 targetPosition)
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

