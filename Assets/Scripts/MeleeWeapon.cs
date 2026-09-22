using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MeleeWeapon : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private LayerMask targetLayers;

    private static readonly int SwingTrigger = Animator.StringToHash("Swing");

    private Animator animator;
    private Collider hitbox;
    private readonly List<Health> alreadyHit = new List<Health>();

    public bool IsSwinging { get; private set; }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        hitbox = GetComponentInChildren<Collider>();
        hitbox.enabled = false;
    }

    private void OnDisable()
    {
        IsSwinging = false;
        hitbox.enabled = false;
    }

    public void Swing()
    {
        if (IsSwinging)
        {
            return;
        }

        IsSwinging = true;
        animator.SetTrigger(SwingTrigger);
    }

    public void HitboxOn()
    {
        alreadyHit.Clear();
        hitbox.enabled = true;
    }

    public void HitboxOff()
    {
        hitbox.enabled = false;
    }

    public void SwingEnd()
    {
        hitbox.enabled = false;
        IsSwinging = false;
        animator.ResetTrigger(SwingTrigger);
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
        if (parry != null && parry.IsParrying)
        {
            parry.OnSuccessfulParry();
            alreadyHit.Add(target);
            return;
        }

        alreadyHit.Add(target);
        target.TakeDamage(damage);
    }
}