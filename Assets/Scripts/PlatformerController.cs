using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlatformerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float crouchSpeed = 3f;
    public float crawlSpeed = 1.2f;
    public float gravity = -25f;
    public float jumpHeight = 2.5f;
    public float doubleJumpHeight = 1.25f;
    public float crouchJumpHeight = 3.5f;

    [Header("Capsule Heights")]
    public float standingHeight = 2f;
    public float crouchingHeight = 1.2f;
    public float crawlingHeight = 0.8f;
    public float heightSmooth = 8f;

    [Header("Fall / Landing")]
    public float fallThreshold = -10f;

    [Header("References")]
    public Transform cameraTransform;
    public Animator animator;

    private PlayerControls controls;
    private CharacterController controller;

    private Vector2 moveInput;
    private bool crouchHeld;

    private bool isCrouching;
    private bool isCrawling;

    private Vector3 velocity;               // vertical velocity
    private Vector3 horizontalVelocity;     // our own tracked horizontal velocity
    private bool canDoubleJump;
    private bool jumpStarted;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        controls = new PlayerControls();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Jump.performed += ctx => StartJump();

        controls.Player.Crouch.performed += ctx => crouchHeld = true;
        controls.Player.Crouch.canceled += ctx => crouchHeld = false;
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Update()
    {
        UpdateCrouchState();
        HandleMovement();
        ApplyGravity();

        // Apply full movement
        Vector3 finalMove = horizontalVelocity + new Vector3(0, velocity.y, 0);
        controller.Move(finalMove * Time.deltaTime);

        UpdateAnimator();
    }

    // ---------------------
    // CROUCH / CRAWL LOGIC (hold button, grounded only)
    // ---------------------
    void UpdateCrouchState()
    {
        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        if (controller.isGrounded && crouchHeld) // Only crouch/crawl if grounded AND button held
        {
            if (isMoving)
            {
                isCrawling = true;
                isCrouching = false;
            }
            else
            {
                isCrouching = true;
                isCrawling = false;
            }
        }
        else // Not grounded or button released > stand
        {
            isCrouching = false;
            isCrawling = false;
        }

        // Update capsule height
        float targetHeight = standingHeight;
        if (isCrawling) targetHeight = crawlingHeight;
        else if (isCrouching) targetHeight = crouchingHeight;

        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * heightSmooth);

        Vector3 c = controller.center;
        c.y = controller.height / 2f;
        controller.center = c;
    }

    // ---------------------
    // MOVEMENT
    // ---------------------
    void HandleMovement()
    {
        float speed =
            isCrawling ? crawlSpeed :
            isCrouching ? crouchSpeed :
            moveSpeed;

        Vector3 forward = cameraTransform.forward; forward.y = 0; forward.Normalize();
        Vector3 right = cameraTransform.right; right.y = 0; right.Normalize();

        Vector3 moveDir = forward * moveInput.y + right * moveInput.x;

        horizontalVelocity = moveDir * speed;

        // Rotate the model toward movement
        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
        }
        else
        {
            horizontalVelocity = Vector3.zero;
        }
    }

    // ---------------------
    // JUMP / GRAVITY
    // ---------------------
    void StartJump()
    {
        if (isCrouching || isCrawling)
        {
            velocity.y = Mathf.Sqrt(crouchJumpHeight * -2f * gravity);
            jumpStarted = true;
            return;
        }

        if (controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            canDoubleJump = true;
            jumpStarted = true;
        }
        else if (canDoubleJump)
        {
            velocity.y = Mathf.Sqrt(doubleJumpHeight * -2f * gravity);
            canDoubleJump = false;
            jumpStarted = true;
        }
    }

    void ApplyGravity()
    {
        if (!controller.isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        else if (velocity.y < 0)
        {
            velocity.y = -2f; // keep grounded
        }
    }

    // ---------------------
    // ANIMATOR
    // ---------------------
    void UpdateAnimator()
    {
        animator.SetFloat("Speed", horizontalVelocity.magnitude);

        animator.SetBool("IsCrouching", isCrouching);
        animator.SetBool("IsCrawling", isCrawling);

        animator.SetBool("IsGrounded", controller.isGrounded);

        animator.SetFloat("VerticalVelocity", velocity.y);

        if (jumpStarted)
        {
            animator.SetTrigger("JumpTriggered");
            jumpStarted = false;
        }

        if (controller.isGrounded && velocity.y < fallThreshold)
        {
            animator.SetTrigger("LandHard");
        }

        // Smooth transition back to standing (optional, animation already handles this)
        if (!crouchHeld && !isCrawling && !isCrouching)
        {
            animator.SetBool("IsCrouching", false);
            animator.SetBool("IsCrawling", false);
        }
    }
}
