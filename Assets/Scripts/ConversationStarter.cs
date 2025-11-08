using UnityEngine;
using DialogueEditor;

public class ConversationStarter : MonoBehaviour
{
    [SerializeField] private NPCConversation boxConversation;
    [SerializeField] private float faceTurnSpeed = 5f;
    [SerializeField] private Animator npcAnimator;
    [SerializeField] private string talkAnimationBool = "IsTalking";

    private bool canTalk = false;
    private Transform player;
    private Animator playerAnimator;
    private bool isPlayerFacing = false;
    private bool isFacingPlayer = false;

    void Start()
    {
        // Subscribe to Dialogue Editor events
        ConversationManager.OnConversationStarted += HandleConversationStart;
        ConversationManager.OnConversationEnded += HandleConversationEnd;
    }

    void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks
        ConversationManager.OnConversationStarted -= HandleConversationStart;
        ConversationManager.OnConversationEnded -= HandleConversationEnd;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.transform;
            playerAnimator = player.GetComponent<Animator>();
            canTalk = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canTalk = false;
            player = null;
            playerAnimator = null;
        }
    }

    void Update()
    {
        if (canTalk && Input.GetKeyDown(KeyCode.F))
        {
            if (!ConversationManager.Instance.IsConversationActive)
            {
                ConversationManager.Instance.StartConversation(boxConversation);
            }
        }

        if(isFacingPlayer && player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0f;
            Quaternion targetRotation=Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * faceTurnSpeed);
        }

        // Smoothly rotate player to face the NPC during dialogue
        if (isPlayerFacing && player != null)
        {
            Vector3 direction = (transform.position - player.position).normalized;
            direction.y = 0f;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            player.rotation = Quaternion.Slerp(player.rotation, targetRotation, Time.deltaTime * faceTurnSpeed);
        }
    }

    private void HandleConversationStart()
    {
        if (player != null)
        {
            // Disable movement
            var controller = player.GetComponent<PlayerMovement>();
            if (controller != null) controller.enabled = false;

            // Force idle animation
            if (playerAnimator != null)
            {
                playerAnimator.SetFloat("Speed", 0f);  // If you use a blend tree
            }

            // Start facing the NPC
            isPlayerFacing = true;
            isFacingPlayer = true;

            if(npcAnimator!=null)
                npcAnimator.SetBool(talkAnimationBool, true);
        }
    }

    private void HandleConversationEnd()
    {
        if (player != null)
        {
            // Re-enable movement
            var controller = player.GetComponent<PlayerMovement>();
            if (controller != null) controller.enabled = true;

            // Stop facing NPC
            isPlayerFacing = false;
            isFacingPlayer = false;

            if (npcAnimator != null)
                npcAnimator.SetBool(talkAnimationBool, false);
        }
    }
}
