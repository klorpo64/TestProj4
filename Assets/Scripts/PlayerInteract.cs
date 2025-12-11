using UnityEngine;
using TMPro;
using DialogueEditor;
using System.Collections;

[RequireComponent(typeof(PlatformerController))]
public class PlayerInteract : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI interactionText;

    [Header("Rotation Settings")]
    public float rotationSpeed = 5f; // affects smooth rotation

    private Interactable currentTarget;
    private PlatformerController playerMovement;
    private Animator anim;

    private bool isInteracting = false;

    void Start()
    {
        playerMovement = GetComponent<PlatformerController>();
        anim = GetComponent<Animator>();

        if (interactionText != null)
            interactionText.gameObject.SetActive(false);
        else
            Debug.LogError("InteractionText is not assigned in the Inspector!");

        ConversationManager.OnConversationEnded += OnConversationEnded;
    }

    void OnDestroy()
    {
        ConversationManager.OnConversationEnded -= OnConversationEnded;
    }

    void Update()
    {
        // Hide prompt if no target or conversation active
        if ((currentTarget == null || (ConversationManager.Instance != null && ConversationManager.Instance.IsConversationActive))
            && interactionText != null && interactionText.gameObject.activeSelf)
        {
            HidePrompt();
        }

        // Input check (Old Input System)
        if (!isInteracting && currentTarget != null)
        {
            // Keyboard F
            if (Input.GetKeyDown(KeyCode.F))
            {
                Debug.Log("F pressed - attempting to start dialogue");
                StartConversation();
            }

            // Controller A / East
            if (Input.GetKeyDown(KeyCode.JoystickButton1))
            {
                Debug.Log("Controller A (East) pressed - attempting to start dialogue");
                StartConversation();
            }
        }
    }

    // --- Conversation ---
    public void OnConversationStarted()
    {
        isInteracting = true;
        HidePrompt();

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (anim != null)
            anim.SetTrigger("Idle");
    }

    private void OnConversationEnded()
    {
        isInteracting = false;

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (currentTarget != null)
            ShowPrompt();
    }

    // --- NPC Communication ---
    public void SetCurrentTarget(Interactable newTarget)
    {
        currentTarget = newTarget;

        if (!isInteracting && (ConversationManager.Instance == null || !ConversationManager.Instance.IsConversationActive))
            ShowPrompt();
    }

    public void ClearTarget(Interactable leavingTarget)
    {
        if (currentTarget == leavingTarget)
        {
            currentTarget = null;
            HidePrompt();
        }
    }

    // --- UI ---
    public void ShowPrompt()
    {
        if (interactionText == null || isInteracting || currentTarget == null) return;
        interactionText.gameObject.SetActive(true);
    }

    public void HidePrompt()
    {
        if (interactionText != null)
            interactionText.gameObject.SetActive(false);
    }

    // --- Start Dialogue with smooth rotation ---
    private void StartConversation()
    {
        if (currentTarget != null)
        {
            StartCoroutine(FaceEachOtherCoroutine(currentTarget));
        }
    }

    private IEnumerator FaceEachOtherCoroutine(Interactable target)
    {
        float duration = 0.3f; // rotation duration in seconds
        float elapsed = 0f;

        // Initial rotations
        Quaternion startPlayerRot = transform.rotation;
        Vector3 playerDir = target.transform.position - transform.position;
        playerDir.y = 0;
        Quaternion targetPlayerRot = Quaternion.LookRotation(playerDir);

        Quaternion startNpcRot = target.transform.rotation;
        Vector3 npcDir = transform.position - target.transform.position;
        npcDir.y = 0;
        Quaternion targetNpcRot = Quaternion.LookRotation(npcDir);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            transform.rotation = Quaternion.Slerp(startPlayerRot, targetPlayerRot, t);
            target.transform.rotation = Quaternion.Slerp(startNpcRot, targetNpcRot, t);

            yield return null;
        }

        // Ensure exact final rotation
        transform.rotation = targetPlayerRot;
        target.transform.rotation = targetNpcRot;

        // Start dialogue after rotation
        OnConversationStarted();
        target.Interact(this);
    }
}