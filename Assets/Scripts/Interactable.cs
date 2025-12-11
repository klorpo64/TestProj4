using UnityEngine;
using DialogueEditor;

[RequireComponent(typeof(Collider))]
public class Interactable : MonoBehaviour
{
    [Header("Dialogue")]
    public NPCConversation conversation;

    private PlayerInteract playerInside;

    void OnTriggerEnter(Collider other)
    {
        PlayerInteract player = other.GetComponent<PlayerInteract>();
        if (player != null)
        {
            if (ConversationManager.Instance == null || !ConversationManager.Instance.IsConversationActive)
            {
                playerInside = player;
                player.SetCurrentTarget(this);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        PlayerInteract player = other.GetComponent<PlayerInteract>();
        if (player != null && player == playerInside)
        {
            player.ClearTarget(this);
            playerInside = null;
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
            Debug.LogWarning($"{gameObject.name} is missing a Conversation asset!");
        }
    }
}