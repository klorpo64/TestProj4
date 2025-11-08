using UnityEngine;
using DialogueEditor;

public class NPCDialogueHandler : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public NPCConversation conversation;

    [Header("Prompt Settings")]
    public GameObject promptUI;

    [HideInInspector] public bool allowInteraction = true; // 👈 new flag

    private bool playerInRange = false;
    private bool dialogueActive = false;

    private void Start()
    {
        // Always start with prompt hidden
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
        // 🔒 Prevent interaction if disabled externally (e.g. NPC walking)
        if (!allowInteraction)
        {
            if (promptUI != null)
                promptUI.SetActive(false);
            return;
        }

        if (playerInRange && !dialogueActive && Input.GetKeyDown(KeyCode.F))
        {
            StartDialogue();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (allowInteraction && !dialogueActive && promptUI != null)
                promptUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (promptUI != null)
                promptUI.SetActive(false);
        }
    }

    private void StartDialogue()
    {
        if (conversation != null)
            ConversationManager.Instance.StartConversation(conversation);

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

        if (allowInteraction && playerInRange && promptUI != null)
            promptUI.SetActive(true);
    }
}
