using System;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActions : MonoBehaviour
{
    [Header("References")]
    private PlayerAnimation playerAnimation;
    private PlayerMovement playerMovement;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private WeaponType weaponType;
    [SerializeField] private LayerMask enemyMask;
   // private PlayerUIManager playerUIManager;

    [Header("Stats")]
    private float maxHealth;
    private float currentHealth;
    private int maxStamina;
    private int currentStamina;
    private float trueRange;
    private float trueDamage;

    [Header("PlayerAttackConditions")]
    private float holdAttackTimer;
    [SerializeField] private float holdAttackTime = 0.6f;
    public bool isHoldingAttack = false;
    public bool isStrongAttack = false;
    public bool canAttack = true;
    public bool isAttacking = false;

    private float attackWindup;
    public bool IsAlive { get; private set; } = true;
    private bool isBurned = false;
    public bool isPoisoned { get; private set; } = false;
    
    public float GetMaxHealth() => maxHealth;
    public float GetCurrentHealth() => currentHealth;

    public event Action<float> OnHealthChanged;

    private void Awake()
    {
        maxHealth = playerStats.maxHealth;
        currentHealth = maxHealth;
        maxStamina = playerStats.maxStamina;
        currentStamina = maxStamina;
    }

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerAnimation = GetComponent<PlayerAnimation>();
    }

    void Update()
    {
        Attack(); 
    }
    private void Attack()
    {
        var mouse = Mouse.current;
        if (mouse == null) 
            return;
        if (playerMovement.isJumping)
            return;
        if(isAttacking)
            return ;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            if (!canAttack)
                return;

            canAttack = false;
            Debug.Log(canAttack);
            isHoldingAttack = true;
            holdAttackTimer = 0f;

        }
        if(mouse.leftButton.isPressed && isHoldingAttack)
        {
            holdAttackTimer += Time.deltaTime;
        }
        if(mouse.leftButton.wasReleasedThisFrame && isHoldingAttack)
        {
            isHoldingAttack = false;

            isStrongAttack = holdAttackTimer >= holdAttackTime;

            playerMovement.canMove = false;
            playerMovement.canJump = false;
            isAttacking = true;

            switch (weaponType.attackType)
            {
                case AttackType.BareHand:
                    playerAnimation.SetAnimationState(isStrongAttack ? PlayerAnimationState.BareHandStrong : PlayerAnimationState.BareHandAttack);
                    break;
                case AttackType.Sword:
                    playerAnimation.SetAnimationState(isStrongAttack ? PlayerAnimationState.StrongAttack : PlayerAnimationState.Attack);
                    StartCoroutine(DealSwordDamage());
                    break ;
                case AttackType.Ranged:
                    playerAnimation.SetAnimationState(isStrongAttack ? PlayerAnimationState.RangedStrong : PlayerAnimationState.RangedAttack);
                    break ;
                case AttackType.Magic:
                    playerAnimation.SetAnimationState(isStrongAttack ? PlayerAnimationState.MagicStrong : PlayerAnimationState.MagicAttack);
                    break ;
            }

        }
    }

    public IEnumerator OnAttackEnded()
    {

        yield return null;

        while (playerAnimation.GetAnimationTime() < 1f)
            yield return null;

        isStrongAttack = false;
        isHoldingAttack = false;
        isAttacking = false;
        playerMovement.canJump = true;
        playerMovement.canMove = true;
        Debug.Log(playerMovement.canMove + "now move");

        playerAnimation.SetAnimationState(PlayerAnimationState.Idle, true);

        StartCoroutine(AttackCooldown(playerStats.attackCooldown * weaponType.weaponCooldown)); 
    }

    private IEnumerator DealSwordDamage()
    {

        attackWindup = 0.8f;

        yield return new WaitForSeconds(attackWindup);

        trueDamage = playerStats.damage * weaponType.power * (isStrongAttack ? 2f : 1f);
        trueRange = playerStats.attackRange * weaponType.weaponRange;
        if (isStrongAttack)
            trueRange *= 1.5f;

        Vector3 center = transform.position + transform.forward * trueRange;

        Collider[] hitCollider = Physics.OverlapSphere(center, trueRange, enemyMask);
        Debug.Log($"Hit colliders count: {hitCollider.Length}");
        foreach (Collider collider in hitCollider)
        {
            Debug.Log("Hit collider: " + collider.name);
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy == null)
            {
                enemy = collider.GetComponentInParent<Enemy>();
                Debug.LogWarning("No Enemy component found for collider: " + collider.name);
            }
            if (enemy != null)
            {
                Debug.Log($"Enemy layer: {enemy.gameObject.layer}");
                Debug.Log("Enemy detected: " + enemy.name);
                enemy.TakeDamage(trueDamage);

                // Apply knockback 
                enemy.ApplyKnockback(transform.position, playerStats.knockBack * weaponType.weaponKnockBack);
            }
        }
        StartCoroutine(OnAttackEnded());
    }


    private IEnumerator AttackCooldown(float cooldownTime)
    {
        yield return new WaitForSeconds(cooldownTime);
        canAttack = true;
        Debug.Log(canAttack);
    }

    public void OnTakeDamage(float damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
        Debug.Log("Player has: " + currentHealth);
        Debug.Log($"Taking damage: {damage}, currentHealth before: {currentHealth}");
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    #region Status Effects
    public void BurnedStatus(float duration)
    {
        // take damage in ticks over time for the duration in which burn lasts
        if (isBurned || !IsAlive)
        {
            Debug.Log(isBurned + "returning");
            return;
        }

        StartCoroutine(BurnRoutine(duration));
    }

    private IEnumerator BurnRoutine(float duration)
    {
        isBurned = true;
        // activate burn status ui 
        OnBurnStatusChanged?.Invoke(true);
        Debug.Log("Burned: " + isBurned);


        float tickInterval = 1f;    // damage tick every second
        float burnDamage = 0.25f;   // damage per tick 

        float timeElapsed = 0f; // track duration of burned status

        // Prevent first burn damage tick to run at the same time as normal damage call
        yield return new WaitForSeconds(tickInterval);
        timeElapsed += tickInterval;

        while (timeElapsed <= duration && IsAlive)
        {
            // call burn damage every tick interval 
            OnTakeDamage(burnDamage);

            yield return new WaitForSeconds(tickInterval);

            timeElapsed += tickInterval;
        }
        isBurned = false;
        // deactivate burn status UI
        OnBurnStatusChanged?.Invoke(false);
        Debug.Log("Burned: " + isBurned);
        
    }

    public event System.Action<bool> OnBurnStatusChanged; 

    public void PoisonedStatus(float duration)
    {
        // take damage in ticks over time for the duration in which burn lasts
        if (isPoisoned || !IsAlive)
        {
            Debug.Log(isPoisoned + "returning");
            return;
        }

        StartCoroutine(PoisonRoutine(duration));
    }

    private IEnumerator PoisonRoutine(float duration)
    {
        isPoisoned = true;
        // activate posion status UI
        OnPoisonedStatusChanged?.Invoke(true);
        Debug.Log("Poisoned: " + isPoisoned);

        float tickInterval = 1f; // damage tick
        float poisonDamage = 0.25f;   // damage per tick 

        float timeElapsed = 0f; // track duration of burned status

        // Prevent first burn damage tick to run at the same time as normal damage call
        yield return new WaitForSeconds(tickInterval);
        timeElapsed += tickInterval;

        while (timeElapsed <= duration && IsAlive)
        {
            playerMovement.canJump = false;
            playerMovement.canDodge = false;
            // call burn damage every tick interval 
            OnTakeDamage(poisonDamage);

            yield return new WaitForSeconds(tickInterval);

            timeElapsed += tickInterval;
        }
        isPoisoned = false;
        playerMovement.canDodge = true;
        playerMovement.canJump = true;
        // deactivate burn status UI
        OnPoisonedStatusChanged?.Invoke(false);
        Debug.Log("Poisoned: " + isPoisoned);

    }

    public event System.Action<bool> OnPoisonedStatusChanged;

    /// <summary>
    /// forward status change to playerMovement
    /// </summary>
    public void SlowedForward(float duration)
    {
        if (playerMovement  != null) 
            playerMovement.SlowStatus(duration);
    }

    public void ConfusedForward(float duration)
    {
        if (playerMovement != null)
            playerMovement.ConfusedStatus(duration);
    }

    public void ParalyzedForward(float duration)
    {
        if (playerMovement != null)
            playerMovement.ParalyzedStatus(duration);
    }


    #endregion

    private void Die()
    {
        // play die animation 
        playerAnimation.SetAnimationState(PlayerAnimationState.Die);

        IsAlive = false;

        canAttack = false;
        playerMovement.canJump = false;
        playerMovement.canMove = false;

        // GameOver screen

    }

    private void HealPlayer()
    {

    }

    #region Knockback

    public void ApplyKnockback(Vector3 source, float force)
    {
        if (!IsAlive)
            return;

        // get direction of knockback
        Vector3 knockbackDirection = transform.position - source;
        knockbackDirection.Normalize();
        knockbackDirection.y = 0f;

        // account enemies knockback resistance 
        float forceAdjustment = force / playerStats.knockbackResist;
        Vector3 targetPostion = transform.position + knockbackDirection * forceAdjustment;

        // Apply smooth knockback  
        StartCoroutine(SmoothKnockback(targetPostion, 0.2f));
    }

    private IEnumerator SmoothKnockback(Vector3 targetPosition, float duration)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;
    }

    #endregion

    /// <summary>
    /// Chat gpt for debug 
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (weaponType == null || playerStats == null)
            return;

        // Calculate the true damage/range just like in DealSwordDamage
        float displayRange = playerStats.attackRange * weaponType.weaponRange;

        // Optional: If holding attack, show the stronger range
        displayRange *= isStrongAttack ? 1.5f : 1f;

        Vector3 center = transform.position + transform.forward * displayRange;

        // Draw a semi-transparent red sphere
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawSphere(center, displayRange);
    }




}
