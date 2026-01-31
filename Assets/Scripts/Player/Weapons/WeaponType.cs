using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu( fileName = "WeaponType", menuName = "WeaponType/WeaponType")]
public class WeaponType : ScriptableObject
{
    public AttackType attackType;
    public float power = 1f;
    public float weaponRange = 1f;
    public int staminaUse = 1;
    public float weaponCooldown = 1f;

}
public enum AttackType
{
    BareHand,
    Sword,
    Ranged,
    Magic
}