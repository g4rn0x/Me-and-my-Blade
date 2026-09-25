using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
public class Enemy : MonoBehaviour
{
    private enum State { Chase, Attack, Dead }

    [Header("References")]
    [SerializeField] private MeleeWeapon weapon;
    [SerializeField] private Transform target;

    [Header("Attack")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackAngle = 30f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float turnSpeed = 360f;

    [Header("Индикатор парирования")]
    [Tooltip("Объект со спрайтом-индикатором парирования над врагом")]
    [SerializeField] private GameObject parryIndicator;

    [Tooltip("Задержка перед появлением индикатора от начала замаха (сек)")]
    [SerializeField] private float indicatorDelay = 0.12f;

    [Tooltip("Время отображения индикатора (активное окно парирования, сек)")]
    [SerializeField] private float indicatorDuration = 0.2f;

    private NavMeshAgent agent;
    private Health health;
    private State state = State.Chase;
    private float nextAttackTime;
    private Coroutine indicatorCoroutine;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();

        if (weapon == null)
        {
            weapon = GetComponentInChildren<MeleeWeapon>();
        }

        if (parryIndicator != null)
        {
            parryIndicator.SetActive(false);
        }
    }

    private void OnEnable()
    {
        health.onDied.AddListener(Die);
    }

    private void OnDisable()
    {
        health.onDied.RemoveListener(Die);
        StopIndicator();
    }

    private void Start()
    {
        if (target != null)
        {
            return;
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
    }

    private void Update()
    {
        if (state == State.Dead || target == null)
        {
            return;
        }

        switch (state)
        {
            case State.Chase:
                UpdateChase();
                break;
            case State.Attack:
                if (!weapon.IsSwinging)
                {
                    nextAttackTime = Time.time + attackCooldown;
                    agent.updateRotation = true;
                    state = State.Chase;
                }
                break;
        }
    }

    private void UpdateChase()
    {
        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > attackRange)
        {
            agent.isStopped = false;
            agent.SetDestination(target.position);
            return;
        }

        agent.isStopped = true;

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        Quaternion lookRotation = Quaternion.LookRotation(toTarget);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, lookRotation, turnSpeed * Time.deltaTime);

        bool isFacingTarget = Vector3.Angle(transform.forward, toTarget) <= attackAngle;

        if (isFacingTarget && Time.time >= nextAttackTime)
        {
            agent.updateRotation = false;
            state = State.Attack;
            weapon.Swing();

            StartIndicator();
        }
    }

    private void StartIndicator()
    {
        StopIndicator();
        if (parryIndicator != null)
        {
            indicatorCoroutine = StartCoroutine(ParryIndicatorRoutine());
        }
    }

    private void StopIndicator()
    {
        if (indicatorCoroutine != null)
        {
            StopCoroutine(indicatorCoroutine);
            indicatorCoroutine = null;
        }

        if (parryIndicator != null)
        {
            parryIndicator.SetActive(false);
        }
    }

    private IEnumerator ParryIndicatorRoutine()
    {
        yield return new WaitForSeconds(indicatorDelay);

        if (state == State.Attack && parryIndicator != null)
        {
            parryIndicator.SetActive(true);
            yield return new WaitForSeconds(indicatorDuration);
            if (parryIndicator != null)
            {
                parryIndicator.SetActive(false);
            }
        }
    }

    private void Die()
    {
        state = State.Dead;
        StopIndicator();
        agent.enabled = false;
        weapon.gameObject.SetActive(false);
        Destroy(gameObject, 2f);
    }
}