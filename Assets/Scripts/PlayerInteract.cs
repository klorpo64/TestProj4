using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionDistance = 3f;
    public InputAction interactAction;

    private Interactable currentTarget;
    private Interactable lastTarget;

    private void Update()
    {
        CheckForInteractable();
    }

    private void CheckForInteractable()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 1f;
        Ray ray = new Ray(rayOrigin, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();

            if (interactable != null)
            {
                currentTarget = interactable;

                if (lastTarget != currentTarget)
                {
                    lastTarget?.HidePrompt();
                    currentTarget.ShowPrompt();
                    lastTarget = currentTarget;
                }

                Debug.DrawRay(rayOrigin, transform.forward * interactionDistance, Color.green);
                return;
            }
        }

        // No interactable in sight
        if (lastTarget != null)
        {
            lastTarget.HidePrompt();
            lastTarget = null;
        }

        currentTarget = null;
    }

    private void OnEnable()
    {
        interactAction.Enable();
        interactAction.performed += OnInteractPerformed;
    }

    private void OnDisable()
    {
        interactAction.performed -= OnInteractPerformed;
        interactAction.Disable();
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (currentTarget != null)
        {
            InteractWithObject();
        }
    }

    private void InteractWithObject()
    {
        Vector3 direction = currentTarget.transform.position - transform.position;
        direction.y = 0;
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);

        DialogueSystem.instance.ShowDialogue(currentTarget.dialogueText);
    }
}