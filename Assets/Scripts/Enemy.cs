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

    private NavMeshAgent agent;
    private Health health;
    private State state = State.Chase;
    private float nextAttackTime;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        health.onDied.AddListener(Die);
    }

    private void OnDisable()
    {
        health.onDied.RemoveListener(Die);
    }

    private void Start()
    {
        if (target != null) { return; }

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) { target = player.transform; }
    }

    private void Update()
    {
        if (state == State.Dead || target == null) { return; }

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
        }
    }

    private void Die()
    {
        state = State.Dead;
        agent.enabled = false;
        weapon.gameObject.SetActive(false);
        Destroy(gameObject, 2f);
    }
}