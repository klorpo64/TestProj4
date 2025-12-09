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

        // If locked, use the override position/rotation
        if (isLocked)
        {
            transform.position = cutsceneTargetPosition;
            transform.rotation = cutsceneTargetRotation;
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
            // Reset orbit angles when unlocking
            rotX = transform.rotation.eulerAngles.y;
            rotY = transform.rotation.eulerAngles.x;
            if (rotY > 180) rotY -= 360;

            UpdateCameraPosition();
        }
    }

    // Gets the camera's current world position
    public Vector3 GetCurrentCameraPosition()
    {
        return transform.position;
    }

    // Gets the camera's current world rotation angles
    public Vector3 GetCurrentCameraRotation()
    {
        return transform.rotation.eulerAngles;
    }

    // Gets the camera's current stored orbit angles
    public Vector3 GetInitialRotation()
    {
        return new Vector3(rotY, rotX, 0f);
    }

    // Sets the position for the locked state
    public void OverridePosition(Vector3 position)
    {
        cutsceneTargetPosition = position;
    }

    // Sets the rotation for the locked state
    public void OverrideRotation(Quaternion rotation)
    {
        cutsceneTargetRotation = rotation;
    }
}