using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponTest : MonoBehaviour
{
    private PlayerActions playerActions;
    [SerializeField] private WeaponType weaponType;

    private void Start()
    {
        playerActions = FindFirstObjectByType<PlayerActions>();
    }

    public void Interact()
    {
        playerActions.EquipWeapon(weaponType);        
    }
}
