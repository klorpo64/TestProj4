using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class BeachBallController : MonoBehaviour
{
    [Header("Physics Settings")]
    public float mass = 0.2f;
    public float drag = 0.8f;
    public float angularDrag = 0.1f;
    public float bounciness = 0.9f;
    public float friction = 0f;

    [Header("Floating Settings")]
    public float waterLevelY = 0f;       // Y position of water surface
    public float floatForceMultiplier = 10f;

    [Header("Splash Settings")]
    public AudioSource splashAudio;
    private bool hasSplashed = false;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Set Rigidbody properties
        rb.mass = mass;
        rb.linearDamping = drag;
        rb.angularDamping = angularDrag;
        rb.useGravity = true;

        // Setup collider physics material for bounciness
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            PhysicsMaterial mat = new PhysicsMaterial();
            mat.bounciness = bounciness;
            mat.dynamicFriction = friction;
            mat.staticFriction = friction;
            mat.bounceCombine = PhysicsMaterialCombine.Maximum;
            col.material = mat;
        }
    }

    void FixedUpdate()
    {
        // Simple floating: apply upward force if below water level
        if (transform.position.y < waterLevelY)
        {
            float depth = waterLevelY - transform.position.y;
            rb.AddForce(Vector3.up * depth * floatForceMultiplier, ForceMode.Force);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Trigger splash sound on water entry
        if (other.CompareTag("Water") && !hasSplashed)
        {
            if (splashAudio != null)
                splashAudio.Play();

            hasSplashed = true;
        }
    }
}
