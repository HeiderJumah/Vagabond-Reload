using System;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActions : MonoBehaviour
{
    public static PlayerActions Instance;

    [Header("References")]
    private PlayerAnimation playerAnimation;
    private PlayerMovement playerMovement;
    [SerializeField] private PlayerStats playerStats;
    public PlayerStats Stats => playerStats;
    [SerializeField] private WeaponType weaponType;
    public WeaponType Weapon => weaponType;
    [SerializeField] private LayerMask enemyMask;
    private EndScreen endScreen;
    // private PlayerUIManager playerUIManager;

    [Header("Stats")]
    private float maxHealth;
    private float currentHealth;
    private int maxStamina;
    private int currentStamina;
    private float trueRange;
    private float trueDamage;

    [Header("Weapon Active")]
    [SerializeField] private GameObject swordObject;
    [SerializeField] private GameObject bowObject;
    [SerializeField] private GameObject magicObject;
    private WeaponType previousWeaponType;

    [Header("Projectile")]
    [SerializeField] private GameObject projectileObject;
    [SerializeField] private Transform shootPoint;

    [Header("PlayerAttackConditions")]
    private float holdAttackTimer;
    [SerializeField] private float holdAttackTime = 0.2f;
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

    public float GetMaxStamina() => maxStamina;

    public float GetCurrentStamina() => currentStamina;

    public event Action<float> OnHealthChanged;

    public event Action<float> OnStaminaChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        maxHealth = playerStats.maxHealth;
        currentHealth = maxHealth;
        maxStamina = playerStats.maxStamina;
        currentStamina = maxStamina;
    }

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerAnimation = GetComponent<PlayerAnimation>();
        endScreen = GetComponent<EndScreen>();
        previousWeaponType = weaponType;
        UpdateWeaponVisual();
    }

    void Update()
    {
        Attack();
        HealPlayer();
    }

    private void UpdateWeaponVisual()
    {
        if (swordObject == null || bowObject == null || magicObject == null)
            return;

        swordObject.SetActive(false);
        bowObject.SetActive(false);
        magicObject.SetActive(false);

        if (weaponType == null)
            return;

        switch (weaponType.attackType)
        {
            case AttackType.BareHand:
                break;
            case AttackType.Sword:
                swordObject.SetActive(true);
                break;
            case AttackType.Ranged:
                bowObject.SetActive(true);
                break;
            case AttackType.Magic:
                magicObject.SetActive(true);
                break;
        }
        // Update previous weapon type after changing visuals
        previousWeaponType = weaponType;
    }

    private void OnValidate()
    {
        if (weaponType == null || weaponType == previousWeaponType)
            return;

        // Ensure weapon visuals update in editor when changing weapon type
        UpdateWeaponVisual();
    }

    private void Attack()
    {
        var mouse = Mouse.current;
        if (mouse == null) 
            return;
        if (playerMovement.isJumping)
            return;
        if(isAttacking)
            return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            if (!canAttack)
                return;

            canAttack = false;
            Debug.Log(canAttack);
            isHoldingAttack = true;
            holdAttackTimer = 0f;

        }

        // strong attack when holding mouse button 
        if (mouse.leftButton.isPressed && isHoldingAttack)
        {
            holdAttackTimer += Time.deltaTime;

            if(holdAttackTimer >= holdAttackTime)
            {
                isHoldingAttack = false;

                if (currentStamina >= 1) // strong attack requires at least 1 stamina
                {
                    ConsumeStamina(1);
                    TriggerAttack(true);
                }
                else
                {
                    TriggerAttack(false); // not enough stamina for strong attack, perform normal attack instead
                }
            }

        }

        if(mouse.leftButton.wasReleasedThisFrame && isHoldingAttack)
        {
            isHoldingAttack = false ;
            TriggerAttack(false); // normal attack 
        }

    }

    private void TriggerAttack(bool strong)
    {
        isStrongAttack = strong;

        playerMovement.canMove = false ;
        playerMovement.canJump = false ;
        playerMovement.canDodge = false ;
        isAttacking = true ;

        switch(weaponType.attackType)
        {
            case AttackType.BareHand:
                playerAnimation.SetAnimationState(isStrongAttack ? PlayerAnimationState.BareHandStrong : PlayerAnimationState.BareHandAttack);
                StartCoroutine(DealBareHandDamage());
                break;
            case AttackType.Sword:
                playerAnimation.SetAnimationState(isStrongAttack ? PlayerAnimationState.StrongAttack : PlayerAnimationState.Attack);
                StartCoroutine(DealSwordDamage());
                break;
            case AttackType.Ranged:
                playerAnimation.SetAnimationState(isStrongAttack ? PlayerAnimationState.RangedStrong : PlayerAnimationState.RangedAttack);
                StartCoroutine(DealRangedDamage());
                break;
            case AttackType.Magic:
                playerAnimation.SetAnimationState(isStrongAttack ? PlayerAnimationState.MagicStrong : PlayerAnimationState.MagicAttack);
                break;

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
        playerMovement.canDodge = true;
        Debug.Log(playerMovement.canMove + "now move");

        playerAnimation.SetAnimationState(PlayerAnimationState.Idle, true);

        StartCoroutine(AttackCooldown(playerStats.attackCooldown * weaponType.weaponCooldown)); 
    }

    private IEnumerator DealBareHandDamage()
    {
        attackWindup = 0.5f;

        yield return new WaitForSeconds(attackWindup);

        trueDamage = playerStats.damage * weaponType.power * (isStrongAttack ? 1.5f : 1f);
        trueRange = playerStats.attackRange * (isStrongAttack ? 1.5f : weaponType.weaponRange);

        Vector3 center = transform.position + transform.forward * trueRange;
        Collider[] hitCollider = Physics.OverlapSphere(center, trueRange, enemyMask);
        foreach (Collider collider in hitCollider)
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(trueDamage);
                // Apply knockback 
                enemy.ApplyKnockback(transform.position, playerStats.knockBack * (isStrongAttack ? 1f : weaponType.weaponKnockBack));
            }
        }
        StartCoroutine(OnAttackEnded());
    }

    private IEnumerator DealSwordDamage()
    {

        attackWindup = 0.8f;

        yield return new WaitForSeconds(attackWindup);

        trueDamage = playerStats.damage * weaponType.power * (isStrongAttack ? 2f : 1f);
        trueRange = playerStats.attackRange * weaponType.weaponRange;
        if (isStrongAttack)
        {
            trueRange *= 1.5f;

            // Deal Damage
            Collider[] hitCollider = Physics.OverlapSphere(transform.position, trueRange, enemyMask);
            foreach (Collider collider in hitCollider)
            {
                Enemy enemy = collider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(playerStats.damage);
                    enemy.ApplyKnockback(transform.position, playerStats.knockBack * 1.5f);
                }
            }

        }
        else
        {

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
        }
        StartCoroutine(OnAttackEnded());
    }

    private IEnumerator DealRangedDamage()
    {
        attackWindup = 0.5f;

        yield return new WaitForSeconds(attackWindup);

        trueDamage = playerStats.damage * weaponType.power * (isStrongAttack ? 2f : 1f);
        trueRange = playerStats.attackRange * weaponType.weaponRange;

        if(isStrongAttack)
        {
            trueRange = playerStats.attackRange + 4f;
            // perform a close range kick with knockback
            // BoxCast size
            Vector3 BoxExtent = new Vector3(0.3f, 0.3f, 0.2f * trueRange);
            Vector3 origin = transform.position + transform.forward * BoxExtent.z + Vector3.up * 0.5f;
            Collider[] hits = Physics.OverlapBox(origin, BoxExtent, transform.rotation, enemyMask);

            foreach (Collider hitCollider in hits)
            {
                Enemy enemy = hitCollider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(playerStats.damage);
                    enemy.ApplyKnockback(transform.position, playerStats.knockBack * 2f);
                }
            }

        }
        else
        {
            // normal attack = shoot projectile 
            if (projectileObject == null || shootPoint == null || weaponType == null)
                yield break;

            // spawn projectile 
            GameObject projectile = Instantiate(projectileObject, shootPoint.position, shootPoint.rotation);

            // fire in mouse direction / allow up and down aiming
            if (Mouse.current != null)
            {
                Vector3 mousePosition = Mouse.current.position.ReadValue();
                Ray ray = Camera.main.ScreenPointToRay(mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hitInfo))
                {
                    Vector3 targetDirection = hitInfo.point - shootPoint.position;
                    projectile.transform.rotation = Quaternion.LookRotation(targetDirection);
                }
            } 

            PlayerProjectile playerProjectile = projectile.GetComponent<PlayerProjectile>();
            if (playerProjectile != null)
                playerProjectile.Init(this);

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
        //endScreen.ActivateGameOverPanel();
        endScreen.ActivatePanel(false);

        MusicManager.Instance.PlayMusic();

    }

    private void HealPlayer()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (endScreen.IsGameOverPanelActive || endScreen.IsVictoryPanelActive)
            return;

        if (keyboard.pKey.wasPressedThisFrame)
        {
            currentHealth = Mathf.Clamp(currentHealth + 1f, 0, maxHealth);
            OnHealthChanged?.Invoke(currentHealth);

            if(!IsAlive)
            {
                IsAlive = true;

                canAttack = true;
                playerMovement.canJump = true;
                playerMovement.canMove = true;
                playerAnimation.SetAnimationState(PlayerAnimationState.Idle, true);
            }

        }
    }

    private void ConsumeStamina(int amount)
    {
        // return if not enough stamina to consume
        if (currentStamina < amount)
            return;

        currentStamina -= amount;
        OnStaminaChanged?.Invoke(currentStamina);

        StartCoroutine(StaminaRegenDelay(8f));
    }

    private IEnumerator StaminaRegenDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        // regenerate stamina over time after delay
        while (currentStamina < maxStamina)
        {
            currentStamina += 1; // regen 1 stamina per tick
            OnStaminaChanged?.Invoke(currentStamina);
            yield return new WaitForSeconds(delay); 
        }
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
    ///  initially Chat gpt for debug, changed for visualizing different attack types 
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (weaponType == null || playerStats == null)
            return;

        float displayRange = playerStats.attackRange * weaponType.weaponRange;

        Gizmos.color = new Color(1f, 0f, 0f, 0.4f);

        switch(weaponType.attackType)
        {
            case AttackType.BareHand:
                if(isStrongAttack)
                {
                    // Strong attack = bigger forward sphere
                    Gizmos.DrawSphere(transform.position + transform.forward, displayRange * 1.5f);
                }
                else
                {
                    // Normal attack = forward sphere
                    Vector3 center = transform.position + transform.forward * displayRange;
                    Gizmos.DrawSphere(center, displayRange);
                }
                break;
            case AttackType.Sword:
                if (isStrongAttack)
                {
                    displayRange = playerStats.attackRange + 1.5f;
                    // Strong attack = AOE around player
                    Gizmos.DrawSphere(transform.position, displayRange);
                }
                else
                {
                    // Normal attack = forward sphere
                    Vector3 center = transform.position + transform.forward * displayRange;
                    Gizmos.DrawSphere(center, displayRange);
                }
                break;
            case AttackType.Ranged:
                if (isStrongAttack)
                {
                    displayRange = playerStats.attackRange + 4f;

                    Vector3 BoxExtent = new Vector3(0.3f, 0.3f, 0.2f* displayRange) ;
                    Vector3 origin = transform.position + transform.forward * BoxExtent.z + Vector3.up * 0.5f;

                    Matrix4x4 oldMatrix = Gizmos.matrix;

                    Gizmos.color = new Color(1f, 0f, 0f, 0.4f);

                    Gizmos.matrix = Matrix4x4.TRS(origin, transform.rotation, Vector3.one);

                    Gizmos.DrawCube(Vector3.zero, BoxExtent * 2f);

                    Gizmos.matrix = oldMatrix; 
                }
                else
                {
                    // Normal attack = forward ray
                    Gizmos.DrawRay(transform.position, transform.forward * displayRange);
                }
                break;
            case AttackType.Magic:
                break;
        }
    }
}
