using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "PlayerStats/PlayerStats" )]
public class PlayerStats : ScriptableObject
{
    public float speed = 1f;
    public float maxHealth = 5f;
    public float damage = 1f;
    public int maxStamina = 3;
    public float attackRange = 1f;
    public float attackCooldown = 1f; 
}
