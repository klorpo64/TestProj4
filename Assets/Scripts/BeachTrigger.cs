using UnityEngine;
using DialogueEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class BeachTrigger : MonoBehaviour
{
    public NPCSimpleMove friendNPC;
    public NPCConversation notReadyConversation;
    public NPCConversation readyConversation;

    public Image fadeImage;
    public float fadeSpeed = 1f;

    private ConversationManager conversationManager;

    private void Start()
    {
        conversationManager = Object.FindFirstObjectByType<ConversationManager>();
        if (conversationManager == null)
            Debug.LogError("No ConversationManager found!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || conversationManager == null)
            return;

        if (!GameManager.Instance.friendNPCTalkedTo || !friendNPC.hasReachedDestination)
            conversationManager.StartConversation(notReadyConversation);
        else
            conversationManager.StartConversation(readyConversation);
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        if (fadeImage == null) yield break;

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
