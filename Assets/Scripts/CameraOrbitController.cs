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

    void Awake()
    {
        controls = new PlayerControls();

        // Only respond to gamepad input
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
        rotY = 30f; // initial X rotation
        UpdateCameraPosition();
    }

    void LateUpdate()
    {
        if (!target) return;

        rotX += lookInput.x * sensitivity * Time.deltaTime;
        rotY -= lookInput.y * sensitivity * Time.deltaTime;
        rotY = Mathf.Clamp(rotY, minY, maxY);

        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        Quaternion rot = Quaternion.Euler(rotY, rotX, 0f);
        transform.position = target.position + rot * cameraOffset;
        transform.rotation = rot;
    }
}
