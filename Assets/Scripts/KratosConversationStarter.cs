using UnityEngine;
using DialogueEditor;

[RequireComponent(typeof(Collider))]
public class KratosConversationStarter : MonoBehaviour, IInteractable
{
    [Header("Dialogue")]
    public NPCConversation conversation;

    [Header("NPC Animation")]
    public Animator npcAnimator;
    public string talkAnimationBool = "IsTalking";

    private PlayerInteract playerInteract;

    void OnTriggerEnter(Collider other)
    {
        playerInteract = other.GetComponentInParent<PlayerInteract>();
        if (playerInteract != null)
        {
            playerInteract.SetCurrentTarget(this);
        }
    }

    void OnTriggerExit(Collider other)
    {
        PlayerInteract leavingPlayer = other.GetComponentInParent<PlayerInteract>();
        if (leavingPlayer != null)
        {
            leavingPlayer.ClearTarget(this);
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

            if (npcAnimator != null)
                npcAnimator.SetBool(talkAnimationBool, true);

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