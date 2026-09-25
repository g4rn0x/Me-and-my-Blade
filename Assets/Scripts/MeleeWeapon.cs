using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private float parryGraceWindow = 0.08f;

    private static readonly int SwingTrigger = Animator.StringToHash("Swing");

    private Animator animator;
    private Collider hitbox;
    private Rigidbody rb;
    private readonly HashSet<Health> alreadyHit = new HashSet<Health>();

    public bool IsSwinging { get; private set; }
    public event Action<Parry> OnAttackParried;

    private void Awake()
    {
        EnsureReferences();

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
        alreadyHit.Clear();
        EnsureReferences();

        if (animator != null)
        {
            animator.SetTrigger(SwingTrigger);
        }
    }

    public void HitboxOn()
    {
        alreadyHit.Clear();
        EnsureReferences();

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

        if (parry != null && parry.CanParryFacing(transform.position))
        {
            if (parry.IsParrying)
            {
                ResolveParry(parry, target);
                return;
            }

            if (parryGraceWindow > 0f && gameObject.activeInHierarchy)
            {
                alreadyHit.Add(target);
                StartCoroutine(DeferredHitRoutine(target, parry));
                return;
            }
        }

        alreadyHit.Add(target);
        target.TakeDamage(damage);
    }

    private void EnsureReferences()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();
        }

        if (hitbox == null)
        {
            hitbox = GetComponent<Collider>() ?? GetComponentInChildren<Collider>();
        }

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }
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

            if (parry != null && parry.IsParrying && parry.CanParryFacing(transform.position))
            {
                ResolveParry(parry, target);
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (target != null)
        {
            target.TakeDamage(damage);
        }
    }
}