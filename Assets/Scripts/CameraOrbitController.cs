using UnityEngine;
using UnityEngine.InputSystem;

public class CameraOrbitController : MonoBehaviour
{
    [Header("References")]
    public Transform target;

    [Header("Camera Settings")]
    public Vector3 cameraOffset = new Vector3(0f, 3f, -5.5f);
    public float sensitivity = 150f;
    public float minY = -30f;
    public float maxY = 65f;

    [Header("Cutscene Smoothing")]
    // NOTE: These fields are currently unused as GameManager directly controls the camera, 
    // but they remain for configuration flexibility.
    public float cutsceneMoveSpeed = 5f;
    public float cutsceneRotateSpeed = 5f;

    private PlayerControls controls;
    private Vector2 lookInput;

    private float rotX;
    private float rotY;

    // Cutscene Variables
    private bool isLocked = false;
    private Vector3 cutsceneTargetPosition;
    private Quaternion cutsceneTargetRotation;

    void Awake()
    {
        controls = new PlayerControls();
        // Set up input controls
        controls.Player.Look.performed += ctx =>
        {
            if (ctx.control.device is Gamepad)
                lookInput = ctx.ReadValue<Vector2>();
        };
        controls.Player.Look.canceled += ctx =>
        {
            if (ctx.control.device is Gamepad)
                lookInput = Vector2.zero;
        };
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Start()
    {
        // Set initial rotation
        rotX = 0f;
        rotY = 30f;
        UpdateCameraPosition();
    }

    void LateUpdate()
    {
        if (!target) return;

        // --- CRITICAL FIX: If the camera is locked by the GameManager, 
        // return immediately to prevent this script from overriding the cutscene's camera movements.
        if (isLocked)
        {
            return;
        }

        // Apply input for rotation
        rotX += lookInput.x * sensitivity * Time.deltaTime;
        rotY -= lookInput.y * sensitivity * Time.deltaTime;
        rotY = Mathf.Clamp(rotY, minY, maxY);

        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        // Calculate new position based on player and offset
        Quaternion rot = Quaternion.Euler(rotY, rotX, 0f);
        transform.position = target.position + rot * cameraOffset;
        transform.rotation = rot;
    }

    // ------------------------
    // Public Methods for Cutscene Control
    // ------------------------

    // Locks or unlocks the camera input
    public void LockCamera(bool shouldLock)
    {
        isLocked = shouldLock;
        if (!shouldLock)
        {
            // Reset orbit angles when unlocking, using the camera's current rotation
            rotX = transform.rotation.eulerAngles.y;
            rotY = transform.rotation.eulerAngles.x;
            if (rotY > 180) rotY -= 360; // Handle positive/negative angle conversion for clamped rotY

            UpdateCameraPosition();
        }
    }

    public void OverridePosition(Vector3 position) { /* cutsceneTargetPosition = position; */ }
    public void OverrideRotation(Quaternion rotation) { /* cutsceneTargetRotation = rotation; */ }

    // Helper methods
    public Vector3 GetCurrentCameraPosition()
    {
        return transform.position;
    }
    public Vector3 GetCurrentCameraRotation()
    {
        return transform.rotation.eulerAngles;
    }
    public Vector3 GetInitialRotation()
    {
        return new Vector3(rotY, rotX, 0f);
    }
}