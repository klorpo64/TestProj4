using UnityEngine;

public class PlayerRagdoll : MonoBehaviour
{
    [Header("References")]
    public Animator animator;                     // Player animator
    public Rigidbody[] ragdollBodies;             // All rigidbodies for ragdoll
    public Collider[] ragdollColliders;           // All colliders for ragdoll
    public Collider mainCollider;                 // Main character collider
    public Rigidbody mainRigidbody;               // Main character rigidbody
    public MonoBehaviour movementScript;          // The script that controls player movement
    public CameraController cameraController;     // Camera script to freeze

    void Start()
    {
        SetRagdoll(false);
    }

    public void ActivateRagdoll(Vector3 force)
    {
        // Stop animation + player control
        if (movementScript != null) movementScript.enabled = false;
        if (animator != null) animator.enabled = false;

        // Freeze camera follow
        if (cameraController != null)
            cameraController.freezeCamera = true;

        SetRagdoll(true);

        // Apply force to each ragdoll rigidbody
        foreach (Rigidbody rb in ragdollBodies)
        {
            rb.AddForce(force, ForceMode.Impulse);
        }
    }

    void SetRagdoll(bool state)
    {
        // Enable ragdoll bodies + colliders
        foreach (Rigidbody rb in ragdollBodies)
        {
            rb.isKinematic = !state;
        }

        foreach (Collider col in ragdollColliders)
        {
            col.enabled = state;
        }

        // Disable/Enable main control collider & rigidbody
        mainCollider.enabled = !state;
        mainRigidbody.isKinematic = state;
    }
}
