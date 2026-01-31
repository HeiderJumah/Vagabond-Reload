using System.Collections;
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




    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerAnimation = GetComponent<PlayerAnimation>();
        maxHealth = playerStats.maxHealth;
        currentHealth = maxHealth;
        maxStamina = playerStats.maxStamina;
        currentStamina = maxStamina;
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

    /// <summary>
    /// called via animation clip event at the end of the attack
    /// </summary>
    public void OnAttackDamage()
    {
        switch (weaponType.attackType)
        {
            case AttackType.BareHand:
               // DealBareHandDamage();
                break;
            case AttackType.Sword:
                DealSwordDamage();
                playerAnimation.SetAnimationState(PlayerAnimationState.Idle);
                break;
            case AttackType.Ranged:
                // DealRangedDamage();
                break;
            case AttackType.Magic:
                // DealMagivDamage();
                break;
        }

    }
    /// <summary>
    /// called via animation clip event at the end of the attack
    /// </summary>
    public void OnAttackEnded()
    {
        isStrongAttack = false;
        playerMovement.canMove = true;
        playerMovement.canJump = true;
        isAttacking = false;

        StartCoroutine(AttackCooldown(playerStats.attackCooldown * weaponType.weaponCooldown));
        Debug.Log("attack cooldown started");
    }

    private void DealSwordDamage()
    {
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
            }
        }
    }


    private IEnumerator AttackCooldown(float cooldownTime)
    {
        yield return new WaitForSeconds(cooldownTime);
        canAttack = true;
    }

    private void OnTakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    private void Die()
    {
        // play die animation 
        playerAnimation.SetAnimationState(PlayerAnimationState.Die);

        // 

    }

    private void HealPlayer()
    {

    }
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
        // displayRange *= isStrongAttack ? 1.5f : 1f;

        Vector3 center = transform.position + transform.forward * displayRange;

        // Draw a semi-transparent red sphere
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawSphere(center, displayRange);
    }




}
