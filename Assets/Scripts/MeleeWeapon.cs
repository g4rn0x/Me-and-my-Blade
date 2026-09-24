using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Универсальное ядро ближнего боя.
/// Управляет взмахом, хитбоксом и расчетом попаданий/парирований.
/// Не зависит от того, кому принадлежит (игроку или врагу).
/// </summary>
public class MeleeWeapon : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private LayerMask targetLayers;

    [Tooltip("Окно снисходительности (Grace Window): время в секундах после касания хитбокса, в течение которого защищающийся ещё может спарировать удар (Sekiro-style deflect)")]
    [SerializeField] private float parryGraceWindow = 0.1f;

    private static readonly int SwingTrigger = Animator.StringToHash("Swing");

    private Animator animator;
    private Collider hitbox;
    private readonly HashSet<Health> alreadyHit = new HashSet<Health>();

    public bool IsSwinging { get; private set; }

    /// <summary>
    /// Вызывается, когда атака этого оружия была парирована целью.
    /// Используется для вызова оглушения (Stun) или нокбэка атакующего.
    /// </summary>
    public event Action<Parry> OnAttackParried;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        hitbox = GetComponentInChildren<Collider>();
        if (hitbox != null)
        {
            hitbox.enabled = false;
        }
    }

    private void OnDisable()
    {
        IsSwinging = false;
        if (hitbox != null)
        {
            hitbox.enabled = false;
        }
        StopAllCoroutines();
    }

    public void Swing()
    {
        if (IsSwinging)
        {
            return;
        }

        IsSwinging = true;
        if (animator != null)
        {
            animator.SetTrigger(SwingTrigger);
        }
    }

    public void HitboxOn()
    {
        alreadyHit.Clear();
        if (hitbox != null)
        {
            hitbox.enabled = true;
        }
    }

    public void HitboxOff()
    {
        if (hitbox != null)
        {
            hitbox.enabled = false;
        }
    }

    public void SwingEnd()
    {
        if (hitbox != null)
        {
            hitbox.enabled = false;
        }
        IsSwinging = false;
        if (animator != null)
        {
            animator.ResetTrigger(SwingTrigger);
        }
    }

    /// <summary>
    /// Физический триггер попадания по цели.
    /// ВНИМАНИЕ: Название строго OnTriggerEnter (используется проксированием ColliderProxy).
    /// </summary>
    public void OnTriggerEnter(Collider other)
    {
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0)
        {
            return;
        }

        Health target = other.GetComponentInParent<Health>();
        if (target == null || alreadyHit.Contains(target))
        {
            return;
        }

        Parry parry = other.GetComponentInParent<Parry>();

        // Проверяем, смотрит ли защищающийся в сторону атаки
        if (parry != null && parry.CanParryFacing(transform.position))
        {
            // Случай 1: Защищающийся УЖЕ находится в активном окне парирования
            if (parry.IsParrying)
            {
                ResolveParry(parry, target);
                return;
            }

            // Случай 2: Защищающийся ещё не парировал, но мы даём микро-буфер (Sekiro Grace Window)
            if (parryGraceWindow > 0f && gameObject.activeInHierarchy)
            {
                alreadyHit.Add(target);
                StartCoroutine(DeferredHitRoutine(target, parry));
                return;
            }
        }

        // Случай 3: Парирования нет или удар нанесён в спину — мгновенный урон
        alreadyHit.Add(target);
        target.TakeDamage(damage);
    }

    private void ResolveParry(Parry parry, Health target)
    {
        alreadyHit.Add(target);
        parry.OnSuccessfulParry();
        OnAttackParried?.Invoke(parry);
    }

    private IEnumerator DeferredHitRoutine(Health target, Parry parry)
    {
        float timer = 0f;

        while (timer < parryGraceWindow)
        {
            if (target == null)
            {
                yield break;
            }

            // Если за время буфера защищающийся успел нажать парирование лицом к удару
            if (parry != null && parry.IsParrying && parry.CanParryFacing(transform.position))
            {
                ResolveParry(parry, target);
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // Буфер истёк, парирование не было нажато — наносим урон
        if (target != null)
        {
            target.TakeDamage(damage);
        }
    }
}