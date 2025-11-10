using UnityEngine;
using DialogueEditor;

public class NPCDialogueListener : MonoBehaviour
{
    public NPCSimpleMove npc;  // Assign your NPCSimpleMove script here in Inspector

    private void OnEnable()
    {
        ConversationManager.OnConversationEnded += OnDialogueEnd;
    }

    private void OnDisable()
    {
        ConversationManager.OnConversationEnded -= OnDialogueEnd;
    }

    private void OnDialogueEnd()
    {
        if (npc != null)
        {
            npc.PrepareToMove();  // Now this runs AFTER dialogue is truly done
        }
    }
}
