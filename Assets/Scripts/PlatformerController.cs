using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlatformerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 9f;
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
    public float rollSpeed = 16f;
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

    [Header("Slope Settings")]
    public float groundedGraceTime = 0.2f;
    public float gravityStickiness = -5f;

    [Header("References")]
    public Animator animator;
    public Transform cameraTransform;
    public Renderer[] playerRenderers;

    [Header("Charge Effect")]
    public Color chargeColor = Color.white;
    public float flashIntensity = 0.5f;
    public float flashSpeed = 8f;

    [HideInInspector] public bool isGroundedBuffered; // Grounded buffer state
    [HideInInspector] public Vector3 velocity; // Public for coconut collection checks

    private CharacterController controller;
    private PlayerControls controls;

    private Vector2 moveInput;
    private Vector3 horizVelocity;

    private bool crouchHeld;
    private bool isCrouching;
    private bool isCrawling;
    private float crouchHoldTime = 0f;

    private bool canDoubleJump;
    private bool jumpStarted;

    // Dive
    private bool isDiving;
    private Vector3 diveDirection;
    private bool hasDivedThisAir = false;

    // Roll
    private bool isRolling;
    private float rollTimer;
    private Vector3 rollDir;
    private float currentRollSpeed;

    // Hard landing stun
    private bool isStunned = false;
    private float stunTimer = 0f;

    // Grounded buffer
    private float airTimer = 0f;

    // Hit by car
    private bool isHitByCar = false;

    // Collider original values
    private float originalHeight;
    private Vector3 originalCenter;

    // Material emission
    private Material[][] playerMaterials;
    private Color[][] originalEmissionColors;

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

    void Start()
    {
        // Store all materials for emission effects
        if (playerRenderers != null && playerRenderers.Length > 0)
        {
            playerMaterials = new Material[playerRenderers.Length][];
            originalEmissionColors = new Color[playerRenderers.Length][];

            for (int r = 0; r < playerRenderers.Length; r++)
            {
                if (playerRenderers[r] == null) continue;

                playerMaterials[r] = playerRenderers[r].materials;
                originalEmissionColors[r] = new Color[playerMaterials[r].Length];

                for (int m = 0; m < playerMaterials[r].Length; m++)
                {
                    if (playerMaterials[r][m].HasProperty("_EmissionColor"))
                        originalEmissionColors[r][m] = playerMaterials[r][m].GetColor("_EmissionColor");
                }
            }
        }
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Update()
    {
        // Freeze during cutscene
        if (GameManager.Instance != null && GameManager.Instance.gameplayFrozen) return;

        // Block movement if hit by car
        if (isHitByCar) return;

        // Grounded buffer
        if (controller.isGrounded)
        {
            airTimer = 0f;
            isGroundedBuffered = true;
        }
        else
        {
            airTimer += Time.deltaTime;
            isGroundedBuffered = airTimer <= groundedGraceTime;
        }

        // Check hard landing
        CheckHardLanding();

        if (isStunned)
        {
            HandleStun();
            return;
        }

        if (isGroundedBuffered) hasDivedThisAir = false;

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

        UpdateAnimator();
        HandleChargeEffect();
    }

    void HandleStun()
    {
        stunTimer -= Time.deltaTime;
        horizVelocity = Vector3.zero;
        velocity.y = -2f;
        if (stunTimer <= 0f) isStunned = false;
        controller.Move(new Vector3(0, velocity.y, 0) * Time.deltaTime);
        UpdateAnimator();
    }

    void HandleMovement()
    {
        float targetSpeed = isCrawling ? crawlSpeed : isCrouching ? crouchSpeed : moveSpeed;

        Vector3 forward = cameraTransform.forward; forward.y = 0; forward.Normalize();
        Vector3 right = cameraTransform.right; right.y = 0; right.Normalize();
        Vector3 moveDir = forward * moveInput.y + right * moveInput.x;

        horizVelocity = moveDir * targetSpeed;

        if (moveDir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDir), 15f * Time.deltaTime);
    }

    void StartJump()
    {
        if (isStunned || isDiving || (GameManager.Instance != null && GameManager.Instance.gameplayFrozen))
            return;

        if (isGroundedBuffered)
        {
            if (isCrouching || isCrawling)
                Jump(crouchHoldTime >= requiredCrouchHold ? crouchJumpHeight : jumpHeight);
            else
            {
                Jump(jumpHeight);
                canDoubleJump = true;
            }
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
        isGroundedBuffered = false;

        if (crouchHoldTime >= requiredCrouchHold)
            ResetMaterialEmission();
    }

    void ApplyGravity()
    {
        if (!controller.isGrounded)
            velocity.y += gravity * Time.deltaTime;
        else if (velocity.y < 0)
            velocity.y = gravityStickiness;
    }

    void StartDive()
    {
        if (isStunned || isGroundedBuffered || isDiving || hasDivedThisAir) return;

        hasDivedThisAir = true;
        isDiving = true;
        velocity.y = Mathf.Sqrt(diveInitialBounce * -2f * gravity);
        diveDirection = (transform.forward * diveForwardBoost + Vector3.down * 0.25f).normalized;
    }

    void HandleDive()
    {
        horizVelocity = new Vector3(diveDirection.x, 0, diveDirection.z) * diveSpeed;
        velocity.y += gravity * Time.deltaTime * 0.6f;

        if (controller.isGrounded)
        {
            isDiving = false;
            if (moveInput.sqrMagnitude > 0.1f)
                StartRoll();
            else
                horizVelocity = Vector3.zero;
        }
    }

    void StartRoll()
    {
        isRolling = true;
        rollTimer = rollDuration;
        rollDir = new Vector3(diveDirection.x, 0, diveDirection.z).normalized;
        transform.rotation = Quaternion.LookRotation(rollDir);
        currentRollSpeed = rollSpeed;
    }

    void HandleRoll()
    {
        Vector3 forward = cameraTransform.forward; forward.y = 0; forward.Normalize();
        Vector3 right = cameraTransform.right; right.y = 0; right.Normalize();
        Vector3 moveDir = forward * moveInput.y + right * moveInput.x;

        if (moveInput.sqrMagnitude < 0.1f)
        {
            horizVelocity = Vector3.zero;
            currentRollSpeed = 0f;
            isRolling = false;
            return;
        }

        if (rollTimer > 0f)
        {
            rollTimer -= Time.deltaTime;
            if (moveDir.sqrMagnitude > 0.01f)
            {
                horizVelocity = moveDir * currentRollSpeed;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDir), 15f * Time.deltaTime);
            }
        }
        else
        {
            float runTargetSpeed = moveSpeed;
            if (isCrouching || isCrawling) runTargetSpeed *= 1.5f;

            currentRollSpeed = Mathf.MoveTowards(currentRollSpeed, runTargetSpeed, rollDecay * Time.deltaTime);

            if (moveDir.sqrMagnitude > 0.01f)
            {
                horizVelocity = moveDir * currentRollSpeed;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDir), 15f * Time.deltaTime);
            }

            if (currentRollSpeed < runTargetSpeed + 0.1f)
                isRolling = false;
        }
    }

    void CheckHardLanding()
    {
        if (controller.isGrounded && velocity.y <= hardLandingVelocity && !isStunned)
        {
            isStunned = true;
            stunTimer = stunDuration;
            horizVelocity = Vector3.zero;
            isDiving = false;
            isRolling = false;
            velocity.y = 0f;
        }
    }

    void UpdateCrouchState()
    {
        if (isStunned) { ResetCollider(); return; }

        bool moving = moveInput.sqrMagnitude > 0.01f;
        bool forcedCrouch = !CanStand();

        if (isGroundedBuffered && (crouchHeld || forcedCrouch))
        {
            crouchHoldTime += Time.deltaTime;
            if (moving) { isCrawling = true; isCrouching = false; }
            else { isCrouching = true; isCrawling = false; }
            controller.height = originalHeight * crouchHeightMultiplier;
            controller.center = new Vector3(originalCenter.x, originalCenter.y * crouchHeightMultiplier, originalCenter.z);
        }
        else
        {
            crouchHoldTime = 0f;
            ResetMaterialEmission();
            if (!forcedCrouch)
            {
                isCrouching = false; isCrawling = false;
                controller.height = originalHeight; controller.center = originalCenter;
            }
            else
            {
                isCrouching = true; isCrawling = true;
                controller.height = originalHeight * crouchHeightMultiplier;
                controller.center = new Vector3(originalCenter.x, originalCenter.y * crouchHeightMultiplier, originalCenter.z);
            }
        }
    }

    void HandleChargeEffect()
    {
        if (playerRenderers == null || playerRenderers.Length == 0) return;

        if (crouchHoldTime >= requiredCrouchHold && (isCrouching || isCrawling))
        {
            float pulse = (Mathf.Sin(Time.time * flashSpeed) * 0.5f + 0.5f) * flashIntensity;
            Color emission = chargeColor * pulse;

            for (int r = 0; r < playerRenderers.Length; r++)
            {
                if (playerMaterials[r] == null) continue;

                for (int m = 0; m < playerMaterials[r].Length; m++)
                {
                    if (playerMaterials[r][m].HasProperty("_EmissionColor"))
                    {
                        playerMaterials[r][m].SetColor("_EmissionColor", emission);
                        playerMaterials[r][m].EnableKeyword("_EMISSION");
                    }
                }
            }
        }
        else
        {
            ResetMaterialEmission();
        }
    }

    void ResetMaterialEmission()
    {
        if (playerRenderers == null || playerRenderers.Length == 0) return;

        for (int r = 0; r < playerRenderers.Length; r++)
        {
            if (playerMaterials[r] == null) continue;

            for (int m = 0; m < playerMaterials[r].Length; m++)
            {
                if (playerMaterials[r][m].HasProperty("_EmissionColor"))
                    playerMaterials[r][m].SetColor("_EmissionColor", originalEmissionColors[r][m]);
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
        crouchHoldTime = 0f; isCrouching = false; isCrawling = false;
        controller.height = originalHeight; controller.center = originalCenter;
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        float currentSpeed = horizVelocity.magnitude;
        float animationSpeedMultiplier = currentSpeed > 0.01f ? currentSpeed / moveSpeed : 1.0f;

        animator.SetFloat("Speed", currentSpeed);
        animator.SetFloat("AnimationSpeedMultiplier", Mathf.Clamp(animationSpeedMultiplier, 1.0f, 3.0f));

        animator.SetFloat("VerticalVelocity", isGroundedBuffered ? 0f : velocity.y);

        animator.SetBool("IsGrounded", isGroundedBuffered);
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

    // Car hit logic
    public void HandleCarHit()
    {
        if (isHitByCar) return;

        isHitByCar = true;
        controller.enabled = false;
    }
}
