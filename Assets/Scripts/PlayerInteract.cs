using UnityEngine;
using TMPro;
using DialogueEditor;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class PlayerInteract : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI interactionText;

    [Header("Rotation Settings")]
    public float rotationSpeed = 5f;

    private IInteractable currentTarget;
    private Animator anim;

    private bool isInteracting = false;
    private MonoBehaviour[] movementControllers;

    void Start()
    {
        anim = GetComponent<Animator>();

        // Dynamically find all movement controllers
        movementControllers = System.Array.FindAll(GetComponents<MonoBehaviour>(), c =>
            c is PlatformerController || c is PlayerMovement
        );

        if (interactionText != null)
            interactionText.gameObject.SetActive(false);
        else
            Debug.LogError("InteractionText not assigned!");

        ConversationManager.OnConversationEnded += OnConversationEnded;
    }

    void OnDestroy()
    {
        ConversationManager.OnConversationEnded -= OnConversationEnded;
    }

    void Update()
    {
        // Show prompt if a target exists and dialogue is inactive
        if (!isInteracting && currentTarget != null)
        {
            if (!interactionText.gameObject.activeSelf)
                interactionText.gameObject.SetActive(true);
        }
        else
        {
            if (interactionText.gameObject.activeSelf)
                interactionText.gameObject.SetActive(false);
        }

        // Input handling
        if (!isInteracting && currentTarget != null)
        {
            if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.JoystickButton1))
            {
                Debug.Log("Attempting to start dialogue with: " + currentTarget.GetTransform().name);

                bool rotateNpc = currentTarget is Interactable; // Only rotate Interactable NPCs
                StartConversation(rotateNpc);
            }
        }
    }

    // --- Conversation ---
    public void OnConversationStarted()
    {
        isInteracting = true;
        HidePrompt();

        foreach (var controller in movementControllers)
            if (controller != null)
                controller.enabled = false;

        if (anim != null)
            anim.SetTrigger("Idle");
    }

    private void OnConversationEnded()
    {
        isInteracting = false;

        foreach (var controller in movementControllers)
            if (controller != null)
                controller.enabled = true;

        if (currentTarget != null)
            ShowPrompt();
    }

    // --- NPC Communication ---
    public void SetCurrentTarget(IInteractable newTarget)
    {
        currentTarget = newTarget;
    }

    public void ClearTarget(IInteractable leavingTarget)
    {
        if (currentTarget == leavingTarget)
            currentTarget = null;
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

    // --- Start Dialogue with rotation ---
    private void StartConversation(bool rotateNpc)
    {
        if (currentTarget != null)
            StartCoroutine(FacePlayerCoroutine(currentTarget, rotateNpc));
    }

    private IEnumerator FacePlayerCoroutine(IInteractable target, bool rotateNpc)
    {
        float duration = 0.3f;
        float elapsed = 0f;

        Transform targetTransform = target.GetTransform();

        Quaternion startPlayerRot = transform.rotation;
        Vector3 playerDir = targetTransform.position - transform.position;
        playerDir.y = 0;
        Quaternion targetPlayerRot = Quaternion.LookRotation(playerDir);

        Quaternion startNpcRot = targetTransform.rotation;
        Quaternion targetNpcRot = startNpcRot;

        if (rotateNpc)
        {
            Vector3 npcDir = transform.position - targetTransform.position;
            npcDir.y = 0;
            targetNpcRot = Quaternion.LookRotation(npcDir);
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            transform.rotation = Quaternion.Slerp(startPlayerRot, targetPlayerRot, t);

            if (rotateNpc)
                targetTransform.rotation = Quaternion.Slerp(startNpcRot, targetNpcRot, t);

            yield return null;
        }

        transform.rotation = targetPlayerRot;
        if (rotateNpc)
            targetTransform.rotation = targetNpcRot;

        OnConversationStarted();
        target.Interact(this);
    }
}