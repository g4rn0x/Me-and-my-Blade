using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Health))]
public class Enemy : MonoBehaviour
{
    private enum State { Chase, Attack, Dead }

    [Header("References")]
    [SerializeField] private MeleeWeapon weapon;
    [Tooltip("Если пусто, враг найдёт объект с тегом Player")]
    [SerializeField] private Transform target;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float turnSpeed = 360f;
    [SerializeField] private float gravity = -30f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackAngle = 30f;
    [SerializeField] private float attackCooldown = 1.5f;

    private const float GroundedStickVelocity = -2f;

    private CharacterController controller;
    private Health health;
    private State state = State.Chase;
    private float verticalVelocity;
    private float nextAttackTime;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
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
        if (target != null)
        {
            return;
        }

        GameObject player = GameObject.FindWithTag("Player");

        if (player != null)
        {
            target = player.transform;
        }
        else
        {
            Debug.LogWarning("Enemy: не найден объект с тегом Player", this);
        }
    }

    private void Update()
    {
        if (state == State.Dead || target == null)
        {
            return;
        }

        Vector3 horizontalVelocity = Vector3.zero;

        switch (state)
        {
            case State.Chase:
                horizontalVelocity = UpdateChase();
                break;
            case State.Attack:
                if (!weapon.IsSwinging)
                {
                    nextAttackTime = Time.time + attackCooldown;
                    state = State.Chase;
                }
                break;
        }

        Move(horizontalVelocity);
    }

    private Vector3 UpdateChase()
    {
        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;

        if (distance > 0.01f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(toTarget);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, lookRotation, turnSpeed * Time.deltaTime);
        }

        if (distance > attackRange)
        {
            return transform.forward * moveSpeed;
        }

        bool isFacingTarget = Vector3.Angle(transform.forward, toTarget) <= attackAngle;

        if (isFacingTarget && Time.time >= nextAttackTime)
        {
            state = State.Attack;
            weapon.Swing();
        }

        return Vector3.zero;
    }

    private void Move(Vector3 horizontalVelocity)
    {
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = GroundedStickVelocity;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = horizontalVelocity;
        velocity.y = verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }

    private void Die()
    {
        state = State.Dead;
        controller.enabled = false;
        weapon.gameObject.SetActive(false);
        Destroy(gameObject, 2f);
    }
}