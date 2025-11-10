using UnityEngine;
using DialogueEditor;

public class NPCDialogueHandler : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public NPCConversation conversation;

    [Header("Prompt Settings")]
    public GameObject promptUI;

    [HideInInspector] public bool allowInteraction = true;
    [HideInInspector] public bool isRunning = false;

    private bool playerInRange = false;
    private bool dialogueActive = false;

    private void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);

        playerInRange = false;
        dialogueActive = false;
    }

    private void OnEnable()
    {
        ConversationManager.OnConversationStarted += OnConversationStarted;
        ConversationManager.OnConversationEnded += OnConversationEnded;
    }

    private void OnDisable()
    {
        ConversationManager.OnConversationStarted -= OnConversationStarted;
        ConversationManager.OnConversationEnded -= OnConversationEnded;
    }

    private void Update()
    {
        // Disable everything if interaction is blocked or NPC is running
        if (!allowInteraction || isRunning)
        {
            if (promptUI != null && promptUI.activeSelf)
                promptUI.SetActive(false);
            return;
        }

        // Try starting dialogue only if fully allowed
        if (playerInRange && !dialogueActive && Input.GetKeyDown(KeyCode.F))
        {
            StartDialogue();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;

        // Show prompt only if interaction allowed AND not running
        if (allowInteraction && !isRunning && !dialogueActive && promptUI != null)
            promptUI.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;

        if (promptUI != null)
            promptUI.SetActive(false);
    }

    private void StartDialogue()
    {
        // Final safety check
        if (!allowInteraction || isRunning || dialogueActive)
            return;

        if (conversation != null)
            ConversationManager.Instance.StartConversation(conversation);

        dialogueActive = true;

        if (promptUI != null)
            promptUI.SetActive(false);
    }

    private void OnConversationStarted()
    {
        dialogueActive = true;
        if (promptUI != null)
            promptUI.SetActive(false);
    }

    private void OnConversationEnded()
    {
        dialogueActive = false;

        // Only show prompt again if NPC is not running and interaction is allowed
        if (allowInteraction && !isRunning && playerInRange && promptUI != null)
            promptUI.SetActive(true);
    }
}
