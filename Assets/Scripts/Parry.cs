using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Универсальный компонент парирования.
/// Может использоваться как игроком (через ввод), так и элитными врагами (через ИИ).
/// </summary>
public class Parry : MonoBehaviour
{
    [Header("Input (опционально, для игрока)")]
    [SerializeField] private InputAction parryAction;

    [Header("Настройки окна парирования")]
    [Tooltip("Длительность активного окна парирования в секундах")]
    [SerializeField] private float parryWindow = 0.25f;

    [Tooltip("Перезарядка парирования в секундах")]
    [SerializeField] private float parryCooldown = 0.5f;

    [Tooltip("Сектор обзора перед собой, в котором работает парирование (в градусах)")]
    [SerializeField] private float maxParryAngle = 140f;

    [Header("Анимация")]
    [SerializeField] private Animator weaponAnimator;

    private static readonly int ParryTrigger = Animator.StringToHash("Parry");

    public bool IsParrying { get; private set; }

    /// <summary>
    /// Событие успешного парирования (для эффектов, звуков, хит-стопа).
    /// </summary>
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
        // Чтение ввода, если экшен назначен и включен (для игрока)
        if (parryAction != null && parryAction.enabled && parryAction.WasPressedThisFrame())
        {
            TryParry();
        }

        // Автоматическое закрытие окна парирования по истечении времени
        if (IsParrying && Time.time >= windowEndTime)
        {
            IsParrying = false;
        }
    }

    /// <summary>
    /// Попытка активировать парирование.
    /// Может вызываться как из Update игрока, так и напрямую ИИ элитного врага.
    /// </summary>
    public bool TryParry()
    {
        if (Time.time < cooldownEndTime)
        {
            return false;
        }

        cooldownEndTime = Time.time + parryCooldown;
        windowEndTime = Time.time + parryWindow;
        IsParrying = true;

        if (weaponAnimator != null)
        {
            weaponAnimator.SetTrigger(ParryTrigger);
        }

        return true;
    }

    /// <summary>
    /// Проверяет, находится ли источник атаки в секторе обзора парирующего (защита от ударов в спину).
    /// </summary>
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

    /// <summary>
    /// Вызывается оружием при успешном парировании удара.
    /// </summary>
    public void OnSuccessfulParry()
    {
        IsParrying = false;
        OnParrySuccessful?.Invoke();
    }
}