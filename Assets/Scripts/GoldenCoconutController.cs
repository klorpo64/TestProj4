using UnityEngine;
using System.Collections;

public class GoldenCoconutController : MonoBehaviour
{
    [Header("Spawn Settings")]
    public float growDuration = 0.5f;           // time to grow from zero to target
    public float initialSpinSpeed = 720f;       // fast spin during spawn
    public float idleSpinSpeed = 60f;           // slow spin while hovering
    public float bounceHeight = 0.2f;
    public float hoverAmplitude = 0.05f;
    public float hoverSpeed = 1f;

    [Header("Player Freeze & Dance")]
    public float danceDuration = 5f;
    public Transform playerTransform;
    public PlatformerController playerController;
    public Transform cameraTransform;

    [Header("Scale")]
    private Vector3 spawnScale = Vector3.zero;
    private Vector3 targetScale = new Vector3(0.01f, 0.01f, 0.01f);
    private Vector3 originalPosition;

    private bool collected = false;

    private void Start()
    {
        // If coconut already collected, destroy self immediately
        if (GameManager.Instance != null && GameManager.Instance.coconutCollected)
        {
            Destroy(gameObject);
            return;
        }

        originalPosition = transform.position;
        transform.localScale = spawnScale;
        StartCoroutine(SpawnAnimation());
    }

    private IEnumerator SpawnAnimation()
    {
        float t = 0f;
        Vector3 startPos = originalPosition;
        Vector3 endPos = originalPosition + Vector3.up * bounceHeight;

        while (t < growDuration)
        {
            t += Time.deltaTime;
            float p = t / growDuration;

            // Scale
            transform.localScale = Vector3.Lerp(spawnScale, targetScale, p);

            // Bounce
            transform.position = Vector3.Lerp(startPos, endPos, Mathf.Sin(p * Mathf.PI));

            // Fast spin
            transform.Rotate(Vector3.up, initialSpinSpeed * Time.deltaTime);

            yield return null;
        }

        transform.localScale = targetScale;
        transform.position = originalPosition;

        // Start idle spin + hover
        StartCoroutine(IdleAnimation());
    }

    private IEnumerator IdleAnimation()
    {
        while (!collected)
        {
            float yOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
            transform.position = originalPosition + new Vector3(0, yOffset, 0);
            transform.Rotate(Vector3.up, idleSpinSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;

        if (other.CompareTag("Player"))
        {
            collected = true;
            StartCoroutine(CollectCoconut());
        }
    }

    private IEnumerator CollectCoconut()
    {
        // Freeze player and camera
        if (playerController != null) playerController.enabled = false;
        if (GameManager.Instance != null) GameManager.Instance.gameplayFrozen = true;

        // Wait until player vertical velocity hits 0 twice
        int zeroHits = 0;
        Vector3 lastVelocity = Vector3.zero;
        while (zeroHits < 2)
        {
            float verticalVel = playerController != null ? playerController.velocity.y : 0f;
            if (Mathf.Abs(verticalVel) < 0.01f && Mathf.Abs(lastVelocity.y) > 0.01f)
                zeroHits++;
            lastVelocity = playerController != null ? playerController.velocity : Vector3.zero;
            yield return null;
        }

        // Make player face camera
        if (playerTransform != null && cameraTransform != null)
        {
            Vector3 lookDir = cameraTransform.position - playerTransform.position;
            lookDir.y = 0;
            playerTransform.rotation = Quaternion.LookRotation(-lookDir);
        }

        // Play dance
        if (playerController != null && playerController.animator != null)
            playerController.animator.SetTrigger("Dance");

        // Wait dance duration
        yield return new WaitForSeconds(danceDuration);

        // Re-enable player control
        if (playerController != null) playerController.enabled = true;
        if (GameManager.Instance != null) GameManager.Instance.gameplayFrozen = false;

        // Update coconut count and mark as collected
        if (GameManager.Instance != null)
            GameManager.Instance.IncrementGoldenCoconutCount();

        // Destroy coconut object
        Destroy(gameObject);
    }
}
