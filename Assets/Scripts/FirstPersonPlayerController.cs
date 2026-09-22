using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonPlayerController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction lookAction;
    [SerializeField] private InputAction jumpAction;

    [Header("Look")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float lookSpeed = 0.1f;
    [SerializeField] private float maxPitch = 80f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float gravity = -30f;
    [SerializeField] private float jumpHeight = 1.5f;
    [Range(0f, 1f)]
    [SerializeField] private float jumpCutMultiplier = 0.5f;
    [SerializeField] private float impulseDecay = 30f;
    [SerializeField] private float groundAcceleration = 80f;
    [SerializeField] private float groundDeceleration = 80f;
    [SerializeField] private float airAcceleration = 20f;
    [SerializeField] private float airDeceleration = 5f;

    private const float GroundedStickVelocity = -2f;

    private CharacterController controller;
    private float pitch;
    private float verticalVelocity;
    private Vector3 impulseVelocity;
    private Vector3 horizontalVelocity;
    private Vector3 wallNormal;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        jumpAction.Disable();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void AddImpulse(Vector3 impulse)
    {
        impulseVelocity += new Vector3(impulse.x, 0f, impulse.z);
        verticalVelocity += impulse.y;
    }

    private void Update()
    {
        HandleLook();
        HandleVerticalVelocity();
        MovePlayer();
    }

    private void HandleLook()
    {
        Vector2 look = lookAction.ReadValue<Vector2>();

        transform.Rotate(0f, look.x * lookSpeed, 0f);

        pitch = Mathf.Clamp(pitch - look.y * lookSpeed, -maxPitch, maxPitch);
        playerCamera.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void HandleVerticalVelocity()
    {
        bool grounded = controller.isGrounded;

        if (grounded && verticalVelocity < 0f)
            verticalVelocity = GroundedStickVelocity;

        if (jumpAction.WasPressedThisFrame() && grounded)
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        if (jumpAction.WasReleasedThisFrame() && verticalVelocity > 0f)
            verticalVelocity *= jumpCutMultiplier;

        verticalVelocity += gravity * Time.deltaTime;
    }

    private void MovePlayer()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 targetDirection = transform.right * input.x + transform.forward * input.y;
        targetDirection = Vector3.ClampMagnitude(targetDirection, 1f);
        Vector3 targetVelocity = targetDirection * moveSpeed;

        bool grounded = controller.isGrounded;
        bool hasInput = input.sqrMagnitude > 0.01f;

        float acceleration;
        if (grounded)
        {
            if (!hasInput)
            {
                acceleration = groundDeceleration;
            }
            else
            {
                bool isChangingDirection = Vector3.Dot(horizontalVelocity.normalized, targetVelocity.normalized) < 0.5f;
                acceleration = isChangingDirection ? groundDeceleration : groundAcceleration;
            }
        }
        else
        {
            acceleration = hasInput ? airAcceleration : airDeceleration;
        }

        horizontalVelocity = Vector3.MoveTowards(
            horizontalVelocity, targetVelocity, acceleration * Time.deltaTime);

        impulseVelocity = Vector3.MoveTowards(
            impulseVelocity, Vector3.zero, impulseDecay * Time.deltaTime);

        Vector3 velocity = horizontalVelocity + impulseVelocity;
        velocity.y = verticalVelocity;

        CollisionFlags flags = controller.Move(velocity * Time.deltaTime);

        if ((flags & CollisionFlags.Sides) != 0 && wallNormal != Vector3.zero)
        {
            float angle = Vector3.Angle(-wallNormal, horizontalVelocity.normalized);

            if (angle < 50f)
            {
                horizontalVelocity = Vector3.ProjectOnPlane(horizontalVelocity, wallNormal);
                impulseVelocity = Vector3.ProjectOnPlane(impulseVelocity, wallNormal);
            }

            wallNormal = Vector3.zero;
        }

        if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
        {
            verticalVelocity = 0f;
        }
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.normal.y > 0.4f || hit.normal.y < -0.4f)
        {
            return;
        }
        wallNormal = hit.normal;
    }
}