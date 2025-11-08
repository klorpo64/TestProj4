using UnityEngine;
using DialogueEditor;

public class NPCGoRight : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 2f;
    public Transform destination;

    [Header("References")]
    public Animator animator;
    public Collider interactionCollider;
    public NPCDialogueHandler dialogueHandler;

    private bool shouldWalk = false;
    private bool walking = false;
    private PlayerMovement playerMovement;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerMovement = player.GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        ConversationManager.OnConversationEnded += OnConversationEnded;
    }

    private void OnDisable()
    {
        ConversationManager.OnConversationEnded -= OnConversationEnded;
    }

    public void PrepareToWalk()
    {
        shouldWalk = true;
    }

    private void OnConversationEnded()
    {
        if (shouldWalk)
        {
            shouldWalk = false;
            walking = true;

            // Disable prompt + interaction through DialogueHandler
            if (dialogueHandler != null)
                dialogueHandler.allowInteraction = false;

            // Disable trigger collider to prevent retriggering
            if (interactionCollider != null)
                interactionCollider.enabled = false;

            if (animator != null)
                animator.SetBool("IsWalking", true);

            if (playerMovement != null)
                playerMovement.enabled = false;
        }
        else
        {
            if (playerMovement != null)
                playerMovement.enabled = true;
        }
    }

    private void Update()
    {
        if (walking && destination != null)
        {
            Vector3 direction = (destination.position - transform.position).normalized;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }

            transform.position = Vector3.MoveTowards(transform.position, destination.position, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, destination.position) < 0.05f)
            {
                walking = false;

                if (animator != null)
                    animator.SetBool("IsWalking", false);

                if (playerMovement != null)
                    playerMovement.enabled = true;
            }
        }
    }
}
