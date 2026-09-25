using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
public class Enemy : MonoBehaviour
{
    private enum State { Chase, Attack, Dead }

    [Header("References")]
    [SerializeField] private GameObject weapon;
    [SerializeField] private Transform target;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2.2f;
    [SerializeField] private float attackAngle = 35f;
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private float turnSpeed = 360f;
    [SerializeField] private float lungeSpeed = 1.5f;

    [Header("Parry Indicator")]
    [SerializeField] private GameObject parryIndicator;
    [SerializeField] private float indicatorDelay = 0.18f;
    [SerializeField] private float indicatorDuration = 0.20f;

    private NavMeshAgent agent;
    private Health health;
    private MeleeWeapon currentWeapon;
    private State state = State.Chase;
    private float nextAttackTime;
    private Coroutine indicatorCoroutine;

    public MeleeWeapon CurrentWeapon
    {
        get
        {
            if (currentWeapon == null && weapon != null)
            {
                currentWeapon = weapon.GetComponentInChildren<MeleeWeapon>();
            }
            if (currentWeapon == null)
            {
                currentWeapon = GetComponentInChildren<MeleeWeapon>();
            }
            return currentWeapon;
        }
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();

        RefreshWeapon();

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
                if (CurrentWeapon != null && CurrentWeapon.IsSwinging)
                {
                    Vector3 toTarget = target.position - transform.position;
                    toTarget.y = 0f;
                    if (toTarget.sqrMagnitude > 1.8f * 1.8f)
                    {
                        agent.Move(transform.forward * (lungeSpeed * Time.deltaTime));
                    }
                }
                else
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
        agent.velocity = Vector3.zero;

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

            if (CurrentWeapon != null)
            {
                CurrentWeapon.Swing();
            }

            StartIndicator();
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
        if (weapon != null)
        {
            weapon.SetActive(false);
        }
        Destroy(gameObject, 2f);
    }
}