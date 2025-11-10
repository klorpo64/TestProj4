using UnityEngine;
using DialogueEditor;

public class NPCSimpleMove : MonoBehaviour
{
    public Transform destination;
    public float moveSpeed = 3f;
    public Animator animator;
    public float rotationSpeed = 5f; // how fast the NPC rotates toward movement

    private bool movementPending = false;
    private bool isMoving = false;

    void OnEnable()
    {
        ConversationManager.OnConversationEnded += OnConversationEnded;
    }

    void OnDisable()
    {
        ConversationManager.OnConversationEnded -= OnConversationEnded;
    }

    // Called from the Event Node in the dialogue
    public void PrepareToMove()
    {
        movementPending = true;
    }

    private void OnConversationEnded()
    {
        if (movementPending)
        {
            isMoving = true;
            movementPending = false;

            if (animator != null)
                animator.SetBool("isRunning", true);
        }
    }

    void Update()
    {
        if (isMoving && destination != null)
        {
            Vector3 direction = (destination.position - transform.position).normalized;

            if (direction != Vector3.zero)
            {
                // Rotate smoothly toward movement direction
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
            }

            // Move toward destination
            transform.position = Vector3.MoveTowards(transform.position, destination.position, moveSpeed * Time.deltaTime);

            // Check if reached destination
            if (Vector3.Distance(transform.position, destination.position) < 0.1f)
            {
                isMoving = false;

                if (animator != null)
                    animator.SetBool("isRunning", false);
            }
        }
    }
}
