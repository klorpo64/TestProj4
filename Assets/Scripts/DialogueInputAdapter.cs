using UnityEngine;
using UnityEngine.InputSystem;
using DialogueEditor; // Import the Dialogue Editor namespace

public class DialogueInputAdapter : MonoBehaviour
{
    // --- Link these in the Inspector ---
    [Header("Input Actions")]
    public InputAction continueAction;   // Bound to East Button (B/Circle/A)
    public InputAction navigateYAction;  // Bound to D-Pad/Left Stick Y-Axis

    [Header("Component References")]
    // Reference to the Dialogue Editor component
    public ConversationManager conversationManager;
    // Reference to the player's movement script to enable/disable movement
    public PlatformerController playerController;

    [Header("Navigation Settings")]
    public float navigationThreshold = 0.5f; // Minimum stick deflection for a move
    public float navigationCooldown = 0.2f;  // Delay between input processing
    private float lastNavigationTime = 0f;


    void Awake()
    {
        // Get the ConversationManager instance if not linked manually
        if (conversationManager == null)
        {
            conversationManager = ConversationManager.Instance;
        }

        // Get the PlatformerController from the parent/self if not linked
        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlatformerController>();
        }
    }

    // --- Input Binding ---
    void OnEnable()
    {
        continueAction.Enable();
        navigateYAction.Enable();

        continueAction.performed += OnContinuePerformed;
        navigateYAction.performed += OnNavigateYPerformed;

        // Subscribe to Dialogue Editor's events to manage player control state
        ConversationManager.OnConversationStarted += DisablePlayerControls;
        ConversationManager.OnConversationEnded += EnablePlayerControls;
    }

    void OnDisable()
    {
        continueAction.performed -= OnContinuePerformed;
        navigateYAction.performed -= OnNavigateYPerformed;

        continueAction.Disable();
        navigateYAction.Disable();

        // Unsubscribe from Dialogue Editor events
        ConversationManager.OnConversationStarted -= DisablePlayerControls;
        ConversationManager.OnConversationEnded -= EnablePlayerControls;
    }


    // ----------------------------------------------------
    // INPUT HANDLERS
    // ----------------------------------------------------

    private void OnContinuePerformed(InputAction.CallbackContext context)
    {
        if (conversationManager != null && conversationManager.IsConversationActive)
        {
            // The East Button (B/Circle/A) is used for all confirmations in Dialogue Editor.
            // This method handles advancing text or selecting the currently highlighted option.
            conversationManager.PressSelectedOption();
        }
    }

    private void OnNavigateYPerformed(InputAction.CallbackContext context)
    {
        if (conversationManager == null || !conversationManager.IsConversationActive) return;

        float navigationValue = context.ReadValue<float>();

        // Check for cooldown and sufficient stick deflection
        if (Time.time > lastNavigationTime + navigationCooldown)
        {
            // Navigate Down (Y = -1)
            if (navigationValue < -navigationThreshold)
            {
                conversationManager.SelectNextOption();
                lastNavigationTime = Time.time;
            }
            // Navigate Up (Y = +1)
            else if (navigationValue > navigationThreshold)
            {
                conversationManager.SelectPreviousOption();
                lastNavigationTime = Time.time;
            }
        }
    }


    // ----------------------------------------------------
    // PLAYER CONTROL MANAGEMENT
    // ----------------------------------------------------

    private void DisablePlayerControls()
    {
        if (playerController != null)
        {
            playerController.enabled = false;
            // You might also want to stop player momentum here:
            // playerController.horizVelocity = Vector3.zero; 
        }
    }

    private void EnablePlayerControls()
    {
        if (playerController != null)
        {
            playerController.enabled = true;
        }
    }
}