using UnityEngine;
using System.Collections;

public class CrateTriggerHit : MonoBehaviour
{
    [Header("Crate Settings")]
    public float requiredSpeed = 3f; // Minimum velocity needed to damage the player

    [Header("Hit Settings")]
    public float launchForce = 10f;
    public AudioSource hitSound;
    public CameraShake cameraShake;
    public ScreenFadeAndReload screenFade;

    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.3f;
    public float slowmoTimeScale = 0.3f;
    public float slowmoDuration = 0.4f;

    // Cached components
    private Rigidbody rb;
    private bool hasHit = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("CrateTriggerHit requires a Rigidbody on this object.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit || rb == null)
            return;

        // Check crate speed BEFORE doing any logic
        float speed = rb.linearVelocity.magnitude;

        if (speed < requiredSpeed)
        {
            // Too slow acts like a harmless bump
            return;
        }

        // Check if we hit the player ragdoll root
        PlayerRagdoll playerRagdoll = other.GetComponentInParent<PlayerRagdoll>();
        if (playerRagdoll == null)
            return;

        hasHit = true;

        PlatformerController playerController = playerRagdoll.GetComponent<PlatformerController>();
        CameraOrbitController cameraController = Camera.main.GetComponent<CameraOrbitController>();

        if (playerController == null || cameraController == null)
            return;

        // Lock camera
        cameraController.LockCamera(true);
        cameraController.OverridePosition(cameraController.transform.position);
        cameraController.OverrideRotation(cameraController.transform.rotation);

        // Disable player control
        playerController.enabled = false;

        // Disable character controller to allow ragdoll
        CharacterController cc = playerController.GetComponent<CharacterController>();
        if (cc != null)
            cc.enabled = false;

        // Launch direction (crate > player)
        Vector3 direction = (other.transform.position - transform.position).normalized;
        playerRagdoll.ActivateRagdoll(direction * launchForce);

        // Sound + camera shake
        hitSound?.Play();
        if (cameraShake != null)
            StartCoroutine(cameraShake.Shake(shakeDuration, shakeMagnitude));

        // Slowmo and fade
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        Time.timeScale = slowmoTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSecondsRealtime(slowmoDuration);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        screenFade?.FadeAndRestart();
    }
}