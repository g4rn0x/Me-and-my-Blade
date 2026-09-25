using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Health))]
public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private InputAction attackAction;
    [SerializeField] private GameObject weapon;

    private Health health;
    private MeleeWeapon currentWeapon;

    public MeleeWeapon CurrentWeapon
    {
        get
        {
            if (currentWeapon == null)
            {
                RefreshWeapon();
            }
            return currentWeapon;
        }
    }

    private void Awake()
    {
        health = GetComponent<Health>();
        RefreshWeapon();
    }

    private void OnEnable()
    {
        attackAction.Enable();
        health.onDied.AddListener(OnDied);
    }

    private void OnDisable()
    {
        attackAction.Disable();
        health.onDied.RemoveListener(OnDied);
    }

    private void Update()
    {
        if (attackAction.WasPressedThisFrame() && CurrentWeapon != null)
        {
            CurrentWeapon.Swing();
        }
    }

    public void SetWeapon(GameObject newWeapon)
    {
        weapon = newWeapon;
        RefreshWeapon();
    }

    private void RefreshWeapon()
    {
        currentWeapon = weapon != null
            ? weapon.GetComponentInChildren<MeleeWeapon>()
            : GetComponentInChildren<MeleeWeapon>();
    }

    private void OnDied()
    {
        GetComponent<FirstPersonPlayerController>().enabled = false;
        enabled = false;
    }
}