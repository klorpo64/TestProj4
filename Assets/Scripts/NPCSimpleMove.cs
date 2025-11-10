using UnityEngine;

public class NPCSimpleMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform destination;
    public float moveSpeed = 3f;
    public Animator animator;
    public float rotationSpeed = 5f;

    [Header("State")]
    public bool hasReachedDestination = false;

    private bool isMoving = false;

    // Reference to dialogue handler
    private NPCDialogueHandler dialogueHandler;

    private void Awake()
    {
        // Try to find the dialogue handler on the same GameObject
        dialogueHandler = GetComponent<NPCDialogueHandler>();
    }

    private void Update()
    {
        if (isMoving && !hasReachedDestination)
        {
            MoveToDestination();
        }
    }

    // Called from Dialogue Editor at the end of dialogue
    public void PrepareToMove()
    {
        isMoving = true;

        // Tell dialogue handler NPC is running
        if (dialogueHandler != null)
            dialogueHandler.isRunning = true;

        if (animator != null)
            animator.SetBool("isRunning", true);
    }

    private void MoveToDestination()
    {
        Vector3 direction = (destination.position - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position, destination.position, moveSpeed * Time.deltaTime);

        if (direction != Vector3.zero && animator != null)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, destination.position) < 0.1f)
        {
            isMoving = false;
            hasReachedDestination = true;

            if (animator != null)
                animator.SetBool("isRunning", false);

            // Tell dialogue handler NPC stopped running
            if (dialogueHandler != null)
                dialogueHandler.isRunning = false;

            gameObject.SetActive(false);
        }
    }
}
