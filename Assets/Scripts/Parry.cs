using UnityEngine;
using UnityEngine.InputSystem;

public class Parry : MonoBehaviour
{
    [SerializeField] private InputAction parryAction;
    [SerializeField] private GameObject parryEffect;
    [SerializeField] private float parryWindow = 0.3f;
    [SerializeField] private float parryCooldown = 1f;
    [SerializeField] private Animator weaponAnimator;

    public bool IsParrying { get; private set; }

    private float windowEndTime;
    private float cooldownEndTime;

    private void OnEnable()
    {
        parryAction.Enable();
    }

    private void OnDisable()
    {
        parryAction.Disable();
        SetParrying(false);
    }

    private void Update()
    {
        if (parryAction.WasPressedThisFrame() && Time.time >= cooldownEndTime)
        {
            SetParrying(true);
            windowEndTime = Time.time + parryWindow;
            cooldownEndTime = Time.time + parryCooldown;
        }

        if (IsParrying && Time.time >= windowEndTime)
        {
            SetParrying(false);
        }
    }

    public void OnSuccessfulParry()
    {
        SetParrying(false);
    }

    private void SetParrying(bool value)
    {
        IsParrying = value;

        if (parryEffect != null)
        {
            parryEffect.SetActive(value);
        }
        
        if (weaponAnimator != null)
        {
            if (value)
            {
                weaponAnimator.SetTrigger("Parry");
            }
        }
    }
}