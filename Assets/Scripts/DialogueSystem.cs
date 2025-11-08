using UnityEngine;
using TMPro; // or using TMPro if using TextMeshPro

public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem instance;

    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public TMP_Text continuePrompt;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        dialoguePanel.SetActive(false);
    }

    public void ShowDialogue(string text)
    {
        dialoguePanel.SetActive(true);
        dialogueText.text = text;
        continuePrompt.gameObject.SetActive(true);
    }

    public void HideDialogue()
    {
        dialoguePanel.SetActive(false);
    }
}
