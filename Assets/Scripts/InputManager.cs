using UnityEngine;
using UnityEngine.InputSystem;
using DialogueEditor;

public class InputManager : MonoBehaviour
{
    private InputSystem_Actions inputActions;

    private Vector2 navigationInput;
    private bool canNavigate = true;
    private float navigationCooldown = 0.25f;

    void Awake()
    {
        inputActions = new InputSystem_Actions();

        inputActions.UI.Navigate.performed += ctx =>
        {
            navigationInput = ctx.ReadValue<Vector2>();
        };

        inputActions.UI.Navigate.canceled += ctx =>
        {
            navigationInput = Vector2.zero;
        };

        inputActions.UI.Submit.performed += ctx =>
        {
            if (IsDialogueActive())
            {
                ConversationManager.Instance.PressSelectedOption();
            }
        };
    }

    void OnEnable()
    {
        inputActions.UI.Enable();
    }

    void OnDisable()
    {
        inputActions.UI.Disable();
    }

    void Update()
    {
        if (!IsDialogueActive()) return;

        HandleNavigation();
    }

    bool IsDialogueActive()
    {
        return ConversationManager.Instance != null &&
               ConversationManager.Instance.IsConversationActive;
    }

    void HandleNavigation()
    {
        if (!canNavigate) return;

        if (navigationInput.y > 0.5f)
        {
            ConversationManager.Instance.SelectNextOption();
            StartCoroutine(NavigationDelay());
        }
        else if (navigationInput.y < -0.5f)
        {
            ConversationManager.Instance.SelectPreviousOption();
            StartCoroutine(NavigationDelay());
        }
    }

    private System.Collections.IEnumerator NavigationDelay()
    {
        canNavigate = false;
        yield return new WaitForSeconds(navigationCooldown);
        canNavigate = true;
    }
}