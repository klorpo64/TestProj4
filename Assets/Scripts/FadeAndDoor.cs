using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeAndDoor : MonoBehaviour
{
    public ScreenFader fader;       // Reference your ScreenFader
    public AudioSource doorAudio;    // Assign your "Door" AudioSource
    public float delayAfterFade = 0f;   // Optional delay before playing sound
    public string nextSceneName = "Path"; // Scene to load
    public float delayAfterSound = 1f;   // Delay before loading next scene

    public void FadeThenPlayDoorAndLoadScene()
    {
        fader.FadeToBlack(() =>
        {
            // Play door sound (with optional small delay)
            if (delayAfterFade > 0)
                StartCoroutine(PlayDoorWithDelay(delayAfterFade));
            else
                doorAudio.Play();

            // Load the next scene after sound delay
            StartCoroutine(LoadSceneAfterDelay(delayAfterSound));
        });
    }

    private IEnumerator PlayDoorWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        doorAudio.Play();
    }

    private IEnumerator LoadSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(nextSceneName);
    }
}
