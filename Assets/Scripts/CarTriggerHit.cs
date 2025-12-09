using UnityEngine;
using System.Collections;

public class CarTriggerHit : MonoBehaviour
{
    // Public references (Assign in Inspector)
    public float launchForce = 10f;
    public AudioSource hitSound;
    public CameraShake cameraShake; // Custom script for camera shake
    public ScreenFadeAndReload screenFade; // Custom script for fading and reloading

    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.3f;
    public float slowmoTimeScale = 0.3f;
    public float slowmoDuration = 0.4f;

    // Private references for controllers
    private PlatformerController playerController;
    private CameraOrbitController cameraController;
    private bool hasHit = false; // Flag to prevent multiple hits

    private void OnTriggerEnter(Collider other)
    {
        // Find the player's root object with the PlayerRagdoll script
        PlayerRagdoll playerRagdoll = other.GetComponentInParent<PlayerRagdoll>();

        if (playerRagdoll == null || hasHit)
        {
            return;
        }

        // Get the core movement/camera controllers
        playerController = playerRagdoll.GetComponent<PlatformerController>();
        cameraController = Camera.main.GetComponent<CameraOrbitController>();

        if (playerController != null && cameraController != null)
        {
            hasHit = true;

            // --- CORE HIT SEQUENCE ---

            // 1. Lock camera and set it to hold its current position (FIX for teleport)
            cameraController.LockCamera(true);
            cameraController.OverridePosition(cameraController.transform.position);
            cameraController.OverrideRotation(cameraController.transform.rotation);

            // 2. Disable the movement script
            playerController.enabled = false;

            // 3. Disable the CharacterController to enable ragdoll physics
            CharacterController cc = playerController.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
            }

            // 4. Launch player into ragdoll mode
            Vector3 direction = (other.transform.position - transform.position).normalized;
            playerRagdoll.ActivateRagdoll(direction * launchForce);

            // Play sound and shake camera
            if (hitSound != null)
                hitSound.Play();

            if (cameraShake != null)
                StartCoroutine(cameraShake.Shake(shakeDuration, shakeMagnitude));

            // Start the cinematic death sequence
            StartCoroutine(DeathSequence());
        }
    }

    private IEnumerator DeathSequence()
    {
        // Set slow motion
        Time.timeScale = slowmoTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // Wait in real time
        yield return new WaitForSecondsRealtime(slowmoDuration);

        // Return to normal time scale
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        // Fade to red and restart the scene
        if (screenFade != null)
        {
            screenFade.FadeAndRestart();
        }
    }
}