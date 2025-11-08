using UnityEngine;

public class Interactable : MonoBehaviour
{
    [TextArea]
    public string dialogueText;

    [Header("Prompt Settings")]
    public GameObject promptPrefab; // assign the "Press F" prefab
    private GameObject promptInstance;

    public void ShowPrompt()
    {
        if (promptPrefab != null && promptInstance == null)
        {
            promptInstance = Instantiate(promptPrefab, transform);
            promptInstance.transform.localPosition = Vector3.up * 2f; // adjust height
        }
    }

    public void HidePrompt()
    {
        if (promptInstance != null)
        {
            Destroy(promptInstance);
        }
    }
}
