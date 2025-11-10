using UnityEngine;
using DialogueEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class BeachTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    public NPCConversation notReadyConversation;
    public NPCConversation readyConversation;

    [Header("Settings")]
    public bool friendTalkedTo = false;
    public bool npcReachedDestination = false;

    [Header("Fade Settings")]
    public Image fadeImage; // Fullscreen black image
    public float fadeSpeed = 1f;

    private ConversationManager conversationManager;

    private void Start()
    {
        // Get ConversationManager instance (Unity 2023+)
        conversationManager = Object.FindFirstObjectByType<ConversationManager>();
        if (conversationManager == null)
            Debug.LogError("No ConversationManager found in the scene!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || conversationManager == null)
            return;

        if (!friendTalkedTo || !npcReachedDestination)
        {
            // Show "can't go" dialogue
            conversationManager.StartConversation(notReadyConversation);
        }
        else
        {
            // Show "ready for beach" dialogue
            conversationManager.StartConversation(readyConversation);
        }
    }

    // This function should be called by the Dialogue Editor **Option Event** on the Yes button
    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        if (fadeImage == null)
        {
            Debug.LogError("Fade Image not assigned!");
            yield break;
        }

        fadeImage.gameObject.SetActive(true);
        Color c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;

        while (c.a < 1f)
        {
            c.a += Time.deltaTime * fadeSpeed;
            fadeImage.color = c;
            yield return null;
        }

        SceneManager.LoadScene(sceneName);
    }
}
