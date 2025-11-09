using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    [Header("References")]
    public GameObject npc;            // NPC to rotate
    public ScreenFader screenFader;   // Script-based fader
    public AudioSource doorSound;     // Door noise AudioSource

    [Header("Settings")]
    public string nextScene = "Walk";
    public float rotationDuration = 1f;  // How long NPC takes to rotate
    public float fadeDuration = 1f;      // Screen fade duration

    private bool isTransitioning = false;

    // Call this from Dialogue Editor UnityEvent for the "Accept Beach" choice
    public void AcceptBeachInvitation()
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionSequence());
        }
    }

    private IEnumerator TransitionSequence()
    {
        isTransitioning = true;

        // 1. Rotate NPC 180 degrees while fading at the same time
        Quaternion startRot = npc.transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, 180f, 0);
        float elapsed = 0f;

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rotationDuration;

            // Rotate NPC
            npc.transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            // Fade screen gradually
            if (screenFader != null)
            {
                float alpha = Mathf.Lerp(0f, 1f, t);
                screenFader.SetAlpha(alpha);
            }

            yield return null;
        }

        npc.transform.rotation = endRot;

        // 2. Ensure screen fully black
        if (screenFader != null)
        {
            screenFader.SetAlpha(1f);
        }

        // 3. Play door sound
        if (doorSound != null)
        {
            doorSound.Play();
        }

        // 4. Wait for door sound to finish
        yield return new WaitForSeconds(doorSound != null ? doorSound.clip.length : 0.5f);

        // 5. Load next scene
        SceneManager.LoadScene(nextScene);
    }
}
