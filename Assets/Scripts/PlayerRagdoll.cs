using UnityEngine;

public class PlayerRagdoll : MonoBehaviour
{
    public Animator animator;
    private Rigidbody[] ragdollBodies;
    private bool isRagdoll = false;

    // Reference to your player movement script
    public MonoBehaviour playerController; // assign your movement script here

    void Awake()
    {
        ragdollBodies = GetComponentsInChildren<Rigidbody>();

        if (animator == null)
            animator = GetComponent<Animator>();

        foreach (Rigidbody rb in ragdollBodies)
        {
            if (rb != null)
                rb.isKinematic = true;
        }
    }

    public void ActivateRagdoll(Vector3 force)
    {
        if (isRagdoll) return;
        isRagdoll = true;

        if (animator != null)
            animator.enabled = false;

        if (playerController != null)
            playerController.enabled = false; // disables player movement

        if (ragdollBodies != null)
        {
            foreach (Rigidbody rb in ragdollBodies)
            {
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.AddForce(force, ForceMode.Impulse);
                }
            }
        }
    }
}
