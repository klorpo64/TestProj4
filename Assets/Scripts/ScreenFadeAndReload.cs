using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ScreenFadeAndReload : MonoBehaviour
{
    public CanvasGroup fadeGroup;
    public float fadeDuration = 1f;

    private void Awake()
    {
        fadeGroup.alpha = 0f; // start clear
    }

    public void FadeAndRestart()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            fadeGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }
}
