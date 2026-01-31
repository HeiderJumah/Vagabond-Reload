using UnityEngine;

[CreateAssetMenu(fileName = "EnemyType", menuName = "Enemy/Enemy Type")]
public class EnemyTypes : ScriptableObject
{
    [Header("Enemy Type Information")]
    public string enemyName;
    public float health;
    public float speed;
    public int damage;
    public float attackRange;
    public float targetingRange;
    public float attackCooldown;
    public GameObject loot;
}

public enum EnemyCategory
{
    slime,
    skeleton,
    bat,
    boss
}
