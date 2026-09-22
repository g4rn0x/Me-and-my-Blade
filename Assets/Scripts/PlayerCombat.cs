using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Health))]
public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private InputAction attackAction;
    [SerializeField] private MeleeWeapon weapon;

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
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
        if (attackAction.WasPressedThisFrame())
        {
            weapon.Swing();
        }
    }

    private void OnDied()
    {
        Debug.Log("Игрок погиб");
        GetComponent<FirstPersonPlayerController>().enabled = false;
        enabled = false;
    }
}