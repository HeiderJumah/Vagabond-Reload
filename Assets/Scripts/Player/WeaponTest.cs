using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponTest : MonoBehaviour
{
    [SerializeField] private WeaponType weaponType;
    public void Interact()
    {
        PlayerActions.Instance.EquipWeapon(weaponType);        
    }
}
