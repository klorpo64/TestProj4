using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlatformerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float crouchSpeed = 3f;
    public float crawlSpeed = 1.2f;

    [Header("Jump Settings")]
    public float jumpHeight = 2.5f;
    public float doubleJumpHeight = 1.2f;
    public float crouchJumpHeight = 3.5f;
    public float gravity = -25f;

    [Header("Dive")]
    public float diveSpeed = 14f;
    public float diveDuration = 0.35f;
    public float diveForwardBoost = 1.5f;

    [Header("Dive Bounce")]
    public float diveInitialBounce = 1.2f;

    [Header("Dive Landing Roll")]
    public float rollSpeed = 10f;
    public float rollDuration = 0.6f;
    public float rollDecay = 5f;

    [Header("Crouch Charge")]
    public float requiredCrouchHold = 3f;

    [Header("Hard Landing")]
    public float hardLandingVelocity = -30f;
    public float stunDuration = 1f;

    [Header("Collider Settings")]
    public float crouchHeightMultiplier = 0.5f;

    [Header("Ceiling Check")]
    public LayerMask ceilingLayers;

    [Header("References")]
    public Animator animator;
    public Transform cameraTransform;

    private CharacterController controller;
    private PlayerControls controls;

    private Vector2 moveInput;
    private Vector3 velocity;
    private Vector3 horizVelocity;

    private bool crouchHeld;
    private bool isCrouching;
    private bool isCrawling;
    private float crouchHoldTime = 0f;

    private bool canDoubleJump;
    private bool jumpStarted;

    // Dive
    private bool isDiving;
    private float diveTimer;
    private Vector3 diveDirection;
    private bool hasDivedThisAir = false;

    // Roll
    private bool isRolling;
    private float rollTimer;
    private Vector3 rollVelocity;

    // Hard landing stun
    private bool isStunned = false;
    private float stunTimer = 0f;

    // Collider original values
    private float originalHeight;
    private Vector3 originalCenter;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        controls = new PlayerControls();

        originalHeight = controller.height;
        originalCenter = controller.center;

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Jump.performed += ctx => StartJump();
        controls.Player.Crouch.performed += ctx => crouchHeld = true;
        controls.Player.Crouch.canceled += ctx => crouchHeld = false;
        controls.Player.Dive.performed += ctx => StartDive();
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Update()
    {
        // ------------------------
        // Handle stun first
        // ------------------------
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            horizVelocity = Vector3.zero;
            velocity.y = -2f;

            if (stunTimer <= 0f)
                isStunned = false;

            controller.Move(new Vector3(0, velocity.y, 0) * Time.deltaTime);
            UpdateAnimator();
            return;
        }

        // Reset dive ability when grounded
        if (controller.isGrounded)
            hasDivedThisAir = false;

        UpdateCrouchState();

        if (isDiving)
            HandleDive();
        else if (isRolling)
            HandleRoll();
        else
            HandleMovement();

        ApplyGravity();

        Vector3 finalMove = horizVelocity + new Vector3(0, velocity.y, 0);
        controller.Move(finalMove * Time.deltaTime);

        // ------------------------
        // Hard landing detection
        // ------------------------
        if (controller.isGrounded && velocity.y <= hardLandingVelocity && !isStunned)
        {
            isStunned = true;
            stunTimer = stunDuration;

            horizVelocity = Vector3.zero;
            isDiving = false;
            isRolling = false;
        }

        UpdateAnimator();
    }

    // ------------------------
    // CROUCH / CRAWL / COLLIDER / CEILING CHECK
    // ------------------------
    void UpdateCrouchState()
    {
        if (isStunned)
        {
            ResetCollider();
            return;
        }

        bool grounded = controller.isGrounded;
        bool moving = moveInput.sqrMagnitude > 0.01f;

        bool forcedCrouch = !CanStand(); // Force crouch if ceiling above

        if (grounded && (crouchHeld || forcedCrouch))
        {
            crouchHoldTime += Time.deltaTime;

            if (moving)
            {
                isCrawling = true;
                isCrouching = false;
            }
            else
            {
                isCrouching = true;
                isCrawling = false;
            }

            controller.height = originalHeight * crouchHeightMultiplier;
            controller.center = new Vector3(originalCenter.x, originalCenter.y * crouchHeightMultiplier, originalCenter.z);
        }
        else
        {
            crouchHoldTime = 0f;

            if (!forcedCrouch)
            {
                // Enough space to stand
                isCrouching = false;
                isCrawling = false;
                controller.height = originalHeight;
                controller.center = originalCenter;
            }
            else
            {
                // Ceiling too low, force crouch
                isCrouching = true;
                isCrawling = true;
                controller.height = originalHeight * crouchHeightMultiplier;
                controller.center = new Vector3(originalCenter.x, originalCenter.y * crouchHeightMultiplier, originalCenter.z);
            }
        }
    }

    bool CanStand()
    {
        Vector3 bottom = transform.position + controller.center - Vector3.up * (controller.height / 2 - controller.radius);
        Vector3 top = bottom + Vector3.up * originalHeight;

        return !Physics.CheckCapsule(bottom, top, controller.radius, ceilingLayers);
    }

    void ResetCollider()
    {
        crouchHoldTime = 0f;
        isCrouching = false;
        isCrawling = false;
        controller.height = originalHeight;
        controller.center = originalCenter;
    }

    // ------------------------
    // MOVEMENT
    // ------------------------
    void HandleMovement()
    {
        float speed =
            isCrawling ? crawlSpeed :
            isCrouching ? crouchSpeed :
            moveSpeed;

        Vector3 forward = cameraTransform.forward; forward.y = 0; forward.Normalize();
        Vector3 right = cameraTransform.right; right.y = 0; right.Normalize();

        Vector3 moveDir = forward * moveInput.y + right * moveInput.x;

        horizVelocity = moveDir * speed;

        if (moveDir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDir), 10f * Time.deltaTime);
        else
            horizVelocity = Vector3.zero;
    }

    // ------------------------
    // JUMP
    // ------------------------
    void StartJump()
    {
        if (isStunned || isDiving || isRolling)
            return;

        if (isCrouching || isCrawling)
        {
            if (crouchHoldTime >= requiredCrouchHold)
                Jump(crouchJumpHeight);
            else
                Jump(jumpHeight);
            return;
        }

        if (controller.isGrounded)
        {
            Jump(jumpHeight);
            canDoubleJump = true;
        }
        else if (canDoubleJump)
        {
            Jump(doubleJumpHeight);
            canDoubleJump = false;
        }
    }

    void Jump(float height)
    {
        velocity.y = Mathf.Sqrt(height * -2f * gravity);
        jumpStarted = true;
    }

    // ------------------------
    // GRAVITY
    // ------------------------
    void ApplyGravity()
    {
        if (!controller.isGrounded)
            velocity.y += gravity * Time.deltaTime;
        else if (velocity.y < 0)
            velocity.y = -2f;
    }

    // ------------------------
    // DIVE
    // ------------------------
    void StartDive()
    {
        if (isStunned || controller.isGrounded || isDiving || isRolling || hasDivedThisAir)
            return;

        hasDivedThisAir = true;
        isDiving = true;
        diveTimer = diveDuration;

        velocity.y = Mathf.Sqrt(diveInitialBounce * -2f * gravity);

        diveDirection = (transform.forward * diveForwardBoost + Vector3.down * 0.25f).normalized;
        horizVelocity = new Vector3(diveDirection.x, 0f, diveDirection.z) * diveSpeed;
        velocity.y += diveDirection.y * diveSpeed;
    }

    void HandleDive()
    {
        diveTimer -= Time.deltaTime;

        horizVelocity = new Vector3(diveDirection.x, 0f, diveDirection.z) * diveSpeed;
        velocity.y += gravity * Time.deltaTime * 0.6f;

        if (controller.isGrounded)
        {
            isDiving = false;
            StartRoll();
            return;
        }

        if (diveTimer <= 0f)
            isDiving = false;
    }

    // ------------------------
    // ROLL
    // ------------------------
    void StartRoll()
    {
        isRolling = true;
        rollTimer = rollDuration;

        Vector3 rollDir = new Vector3(diveDirection.x, 0, diveDirection.z).normalized;
        rollVelocity = rollDir * rollSpeed;
        transform.rotation = Quaternion.LookRotation(rollDir);
    }

    void HandleRoll()
    {
        rollTimer -= Time.deltaTime;

        horizVelocity = rollVelocity;
        rollVelocity = Vector3.Lerp(rollVelocity, Vector3.zero, rollDecay * Time.deltaTime);

        if (rollTimer <= 0f)
        {
            isRolling = false;
            rollVelocity = Vector3.zero;
        }
    }

    // ------------------------
    // ANIMATOR
    // ------------------------
    void UpdateAnimator()
    {
        if (animator == null) return;

        animator.SetFloat("Speed", horizVelocity.magnitude);
        animator.SetFloat("VerticalVelocity", velocity.y);

        animator.SetBool("IsGrounded", controller.isGrounded);
        animator.SetBool("IsCrouching", isCrouching);
        animator.SetBool("IsCrawling", isCrawling);
        animator.SetBool("IsDiving", isDiving);
        animator.SetBool("IsRolling", isRolling);

        if (jumpStarted)
        {
            animator.SetTrigger("JumpTriggered");
            jumpStarted = false;
        }
    }
}
