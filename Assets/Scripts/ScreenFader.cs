using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public Image fadeImage;         // assign the black Image here
    public float fadeDuration = 1f;

    private void Awake()
    {
        if (fadeImage != null)
        {
            SetAlpha(0f); // start transparent
        }
    }

    // Fade fully to black
    public void FadeToBlack(System.Action onComplete = null)
    {
        StartCoroutine(Fade(0f, 1f, onComplete));
    }

    // Fade fully to transparent
    public void FadeFromBlack(System.Action onComplete = null)
    {
        StartCoroutine(Fade(1f, 0f, onComplete));
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, System.Action onComplete)
    {
        float elapsed = 0f;
        Color c = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }

        c.a = endAlpha;
        fadeImage.color = c;
        onComplete?.Invoke();
    }

    // Manually set alpha (for smooth fade during rotation)
    public void SetAlpha(float alpha)
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = Mathf.Clamp01(alpha);
            fadeImage.color = c;
        }
    }
}
