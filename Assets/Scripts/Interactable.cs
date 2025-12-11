using UnityEngine;
using DialogueEditor;

[RequireComponent(typeof(Collider))]
public class Interactable : MonoBehaviour, IInteractable
{
    [Header("Dialogue")]
    public NPCConversation conversation;

    private PlayerInteract playerInteract;

    void OnTriggerEnter(Collider other)
    {
        playerInteract = other.GetComponent<PlayerInteract>();
        if (playerInteract != null)
        {
            playerInteract.SetCurrentTarget(this);
        }
    }

    void OnTriggerExit(Collider other)
    {
        PlayerInteract leavingPlayer = other.GetComponent<PlayerInteract>();
        if (leavingPlayer != null && leavingPlayer == playerInteract)
        {
            playerInteract.ClearTarget(this);
            playerInteract = null;
        }
    }

    public void Interact(PlayerInteract player)
    {
        if (ConversationManager.Instance != null && ConversationManager.Instance.IsConversationActive)
            return;

        if (conversation != null)
        {
            player.OnConversationStarted();
            ConversationManager.Instance.StartConversation(conversation);
        }
        else
        {
            Debug.LogWarning(gameObject.name + " is missing a Conversation asset!");
        }
    }

    public Transform GetTransform()
    {
        return transform;
    }
}