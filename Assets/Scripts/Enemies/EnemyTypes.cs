using UnityEngine;

[CreateAssetMenu(fileName = "EnemyType", menuName = "Enemy/Enemy Type")]
public class EnemyTypes : ScriptableObject
{
    [Header("Enemy Type Information")]
    public string enemyName;
    public float health;
    public float speed;
    public float damage;
    public float attackRange;
    public float targetingRange;
    public float attackCooldown;
    public GameObject loot;
    public EnemyCategory enemyCategory;
    public float attackWindup = 0.4f;
    public float hitKnockbackResistance = 1f; 
    public float patrollingRange;
    public float patrolWaitTime;
}

public enum EnemyCategory
{
    slime,
    rabbit,
    goblin,
    skeleton,
    swordSkeleton,
    bat,
    golemBoss
}
