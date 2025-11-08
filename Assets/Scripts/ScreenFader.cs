using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class ScreenFader : MonoBehaviour
{
    public Image fadeImage;
    public float fadeSpeed = 1f;

    // Fade to black
    public void FadeToBlack(Action onComplete = null)
    {
        StartCoroutine(Fade(1f, onComplete));
    }

    // Fade from black
    public void FadeFromBlack(Action onComplete = null)
    {
        StartCoroutine(Fade(0f, onComplete));
    }

    private IEnumerator Fade(float targetAlpha, Action onComplete)
    {
        Color c = fadeImage.color;
        while (!Mathf.Approximately(c.a, targetAlpha))
        {
            c.a = Mathf.MoveTowards(c.a, targetAlpha, fadeSpeed * Time.deltaTime);
            fadeImage.color = c;
            yield return null;
        }

        onComplete?.Invoke(); // Call callback after fade finishes
    }
}
