using UnityEngine;

public class RagdollTest : MonoBehaviour
{
    public Animator animator;           // assign your player's Animator
    public Rigidbody[] ragdollBodies;  // assign all ragdoll rigidbodies

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Disable animator
            if (animator != null)
                animator.enabled = false;

            // Enable ragdoll
            if (ragdollBodies != null)
            {
                foreach (Rigidbody rb in ragdollBodies)
                {
                    if (rb != null)
                        rb.isKinematic = false;
                }
            }
        }
    }
}
