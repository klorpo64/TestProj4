using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Needed to load scenes
using System.Collections;

public class TravelScreenFader : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 1f;
    public float preFadeDelay = 2f; // Seconds to wait before fading
    public float postFadeDelay = 1f; // Seconds to wait after fade before scene switch

    public void FadeToBlackAndLoadScene()
    {
        StartCoroutine(FadeAndLoadRoutine());
    }

    private IEnumerator FadeAndLoadRoutine()
    {
        // Wait before starting the fade
        yield return new WaitForSeconds(preFadeDelay);

        // Fade to black
        float timer = 0f;
        Color color = fadeImage.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;

        // Wait after fade before changing scene
        yield return new WaitForSeconds(postFadeDelay);

        // Load the next scene
        SceneManager.LoadScene("TravelCutscene");
    }
}
