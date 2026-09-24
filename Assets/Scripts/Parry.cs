using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Parry : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction parryAction;

    [Header("Settings")]
    [SerializeField] private float parryWindow = 0.2f;
    [SerializeField] private float parryCooldown = 0.4f;
    [SerializeField] private float maxParryAngle = 140f;

    [Header("Animation")]
    [SerializeField] private Animator weaponAnimator;

    private static readonly int ParryTrigger = Animator.StringToHash("Parry");

    public bool IsParrying { get; private set; }
    public event Action OnParrySuccessful;

    private float windowEndTime;
    private float cooldownEndTime;

    private void Awake()
    {
        if (weaponAnimator == null)
        {
            weaponAnimator = GetComponentInChildren<Animator>();
        }
    }

    private void OnEnable()
    {
        if (parryAction != null && parryAction.bindings.Count > 0)
        {
            parryAction.Enable();
        }
    }

    private void OnDisable()
    {
        if (parryAction != null)
        {
            parryAction.Disable();
        }
        IsParrying = false;
    }

    private void Update()
    {
        if (parryAction != null && parryAction.enabled && parryAction.WasPressedThisFrame())
        {
            TryParry();
        }

        if (IsParrying && Time.time >= windowEndTime)
        {
            IsParrying = false;
        }
    }

    public bool TryParry()
    {
        if (Time.time < cooldownEndTime)
        {
            return false;
        }

        cooldownEndTime = Time.time + parryCooldown;
        windowEndTime = Time.time + parryWindow;
        IsParrying = true;

        if (weaponAnimator == null)
        {
            weaponAnimator = GetComponentInChildren<Animator>();
        }

        if (weaponAnimator != null)
        {
            weaponAnimator.SetTrigger(ParryTrigger);
        }

        return true;
    }

    public bool CanParryFacing(Vector3 attackerPosition)
    {
        Vector3 toAttacker = attackerPosition - transform.position;
        toAttacker.y = 0f;

        if (toAttacker.sqrMagnitude < 0.0001f)
        {
            return true;
        }

        Vector3 forward = transform.forward;
        forward.y = 0f;

        float angle = Vector3.Angle(forward, toAttacker);
        return angle <= maxParryAngle * 0.5f;
    }

    public void OnSuccessfulParry()
    {
        IsParrying = false;
        OnParrySuccessful?.Invoke();
    }
}