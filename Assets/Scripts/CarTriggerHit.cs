using UnityEngine;

public class CarTriggerHit : MonoBehaviour
{
    public float launchForce = 10f;
    public AudioSource hitSound;
    public CameraShake cameraShake;
    public ScreenFadeAndReload screenFade;

    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.3f;
    public float slowmoTimeScale = 0.3f;
    public float slowmoDuration = 0.4f;

    private void OnTriggerEnter(Collider other)
    {
        PlayerRagdoll player = other.GetComponentInParent<PlayerRagdoll>();
        if (player != null)
        {
            // Launch player into ragdoll
            Vector3 direction = (other.transform.position - transform.position).normalized;
            player.ActivateRagdoll(direction * launchForce);

            // Sound
            if (hitSound != null)
                hitSound.Play();

            // Camera shake
            if (cameraShake != null)
                StartCoroutine(cameraShake.Shake(shakeDuration, shakeMagnitude));

            // Slow-motion then fade and restart
            StartCoroutine(DeathSequence());
        }
    }

    private System.Collections.IEnumerator DeathSequence()
    {
        // Slow motion
        Time.timeScale = slowmoTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSecondsRealtime(slowmoDuration);

        // Return to normal time
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        // Fade to black and restart
        screenFade.FadeAndRestart();
    }
}
