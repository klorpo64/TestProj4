using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class GoldenCoconutController : MonoBehaviour
{
    // Define the regular (final) scale, matching GameManager's REGULAR_SCALE
    private const float REGULAR_SCALE_VALUE = 0.01f;

    [Header("ID & State")]
    private string coconutID;
    public bool isCollected = false;
    public Material collectedMaterial;
    public Material uncollectedMaterial;

    [Header("Spawn Animation")]
    public float spawnDuration = 1.5f;
    public Vector3 targetScale = new Vector3(REGULAR_SCALE_VALUE, REGULAR_SCALE_VALUE, REGULAR_SCALE_VALUE);
    public float initialScale = 0.0000000000001f;
    public float spawnBounceHeight = 1f;
    public float spawnSpinSpeed = 720f;

    private bool isFullyGrown = false;

    [Header("Idle Animation")]
    public float hoverAmplitude = 0.05f;
    public float hoverSpeed = 1f;
    public float rotationSpeed = 60f;

    [Header("Audio")]
    public AudioClip collectSound;
    private AudioSource audioSource;

    private Renderer coconutRenderer;
    private Collider coconutCollider;
    private Vector3 originalPosition;
    private Coroutine idleCoroutine;

    void Awake()
    {
        coconutRenderer = GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>();
        coconutCollider = GetComponent<Collider>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        originalPosition = transform.position;
    }

    // -----------------------------
    // Public Methods
    // -----------------------------
    public void Initialize(string id, bool collectedStatus, bool isCollectedPrefab)
    {
        coconutID = id;
        isCollected = collectedStatus;
        originalPosition = transform.position;

        SetVisuals(!isCollectedPrefab);

        float scaleTolerance = 0.0001f;

        if (!isCollectedPrefab)
        {
            if (transform.localScale.x > targetScale.x - scaleTolerance)
            {
                isFullyGrown = true;
            }
            else
            {
                isFullyGrown = false;
            }
        }
        else
        {
            isFullyGrown = true;
            SetVisuals(true);
        }

        if (coconutCollider != null)
        {
            coconutCollider.enabled = !isCollected && isFullyGrown;
        }
    }

    public void StartSpawnAnimation()
    {
        if (idleCoroutine != null) StopCoroutine(idleCoroutine);
        SetVisuals(true);
        isFullyGrown = false;

        StartCoroutine(SpawnAnimationRoutine());
    }

    public void StartIdleHover()
    {
        if (idleCoroutine == null)
            idleCoroutine = StartCoroutine(IdleHoverRoutine());
    }

    // -----------------------------
    // Private Methods
    // -----------------------------
    private void SetVisuals(bool visible)
    {
        if (coconutRenderer != null)
            coconutRenderer.enabled = visible;

        if (visible && uncollectedMaterial != null)
            coconutRenderer.material = uncollectedMaterial;

        if (!visible && collectedMaterial != null)
            coconutRenderer.material = collectedMaterial;
    }

    private IEnumerator SpawnAnimationRoutine()
    {
        float timer = 0f;
        Vector3 startScale = transform.localScale;

        if (coconutCollider != null) coconutCollider.enabled = false;

        while (timer < spawnDuration)
        {
            timer += Time.deltaTime;
            float t = timer / spawnDuration;

            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            transform.Rotate(Vector3.up, spawnSpinSpeed * Time.deltaTime, Space.World);

            float yOffset = spawnBounceHeight * Mathf.Sin(t * Mathf.PI);
            transform.position = originalPosition + Vector3.up * yOffset;

            yield return null;
        }

        transform.localScale = targetScale;
        transform.position = originalPosition;
        isFullyGrown = true;

        if (coconutCollider != null) coconutCollider.enabled = true;

        StartIdleHover();
    }

    private IEnumerator IdleHoverRoutine()
    {
        float timeOffset = Random.Range(0f, 10f);

        while (true)
        {
            float yOffset = Mathf.Sin(Time.time * hoverSpeed + timeOffset) * hoverAmplitude;
            transform.position = originalPosition + Vector3.up * yOffset;

            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

            yield return null;
        }
    }

    // -----------------------------
    // Collection
    // -----------------------------
    private void OnTriggerEnter(Collider other)
    {
        if (!isFullyGrown || (GameManager.Instance != null && GameManager.Instance.gameplayFrozen)) return;

        if (other.CompareTag("Player")) CollectCoconutImmediate();
    }

    private void CollectCoconutImmediate()
    {
        StopAllCoroutines();

        bool alreadyCollectedInManager = GameManager.Instance.IsCoconutCollected(coconutID);

        if (!alreadyCollectedInManager)
        {
            GameManager.Instance.CollectCoconut(coconutID);
            isCollected = true;
        }

        if (collectSound != null && audioSource != null)
            audioSource.PlayOneShot(collectSound);

        SetVisuals(false);

        Destroy(gameObject, 0.1f);
    }
}