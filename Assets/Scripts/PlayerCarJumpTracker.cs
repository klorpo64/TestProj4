using UnityEngine;

public class PlayerCarJumpTracker : MonoBehaviour
{
    public int carsJumped = 0;
    public GameObject popupPrefab; // Assumed to have a JumpCounterPopup component
    public Transform popupSpawnPoint;
    public int requiredJumps = 5;

    [Header("Car Jump Coconut")]
    public Transform carJumpCoconutSpawnPoint; // Assign in inspector
    public AudioClip[] carJumpSounds;           // Assign a sound for each car jumped

    private const string COCONUT_ID = "Car Hopper";

    private bool countingActive = true;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        // 1. Reset count on scene load (death/restart)
        carsJumped = 0;
        countingActive = true;

        // 2. Check current collection status
        bool collected = GameManager.Instance != null && GameManager.Instance.IsCoconutCollected(COCONUT_ID);

        if (collected)
        {
            // If the golden coconut is already collected, stop counting jumps permanently.
            countingActive = false;

            // 3. Immediately spawn the grey/collected version
            if (carJumpCoconutSpawnPoint != null)
            {
                GameManager.Instance.SpawnGoldenCoconut(
                    COCONUT_ID,
                    carJumpCoconutSpawnPoint,
                    false // Not a challenge spawn (no cutscene for the already-collected version)
                );
            }
        }
        // If not collected, countingActive remains true, and carsJumped remains 0.
    }

    public void RegisterCarJump()
    {
        // Only count if the golden coconut hasn't been collected yet
        if (!countingActive) return;

        carsJumped++;
        Debug.Log($"Car jumped. Count = {carsJumped}");

        // Play jump sound
        if (carJumpSounds != null && carJumpSounds.Length > 0)
        {
            int soundIndex = Mathf.Min(carsJumped - 1, carJumpSounds.Length - 1);
            audioSource.PlayOneShot(carJumpSounds[soundIndex]);
        }

        // Show popup
        if (popupPrefab != null && popupSpawnPoint != null)
        {
            GameObject popup = Instantiate(popupPrefab, popupSpawnPoint.position, Quaternion.identity);
            popup.GetComponent<JumpCounterPopup>()?.Initialize(carsJumped);
        }

        // Check for coconut spawn
        if (carsJumped >= requiredJumps)
        {
            // Stop counting after the challenge is complete
            countingActive = false;

            if (carJumpCoconutSpawnPoint != null)
            {
                Debug.Log("Spawning car jump coconut (Challenge Reward)...");

                GameManager.Instance.SpawnGoldenCoconut(
                    COCONUT_ID,
                    carJumpCoconutSpawnPoint,
                    true // Is challenge spawn (use cutscene logic for golden coconut)
                );
            }
            else
            {
                Debug.LogError("CarJumpCoconutSpawnPoint not assigned!");
            }
        }
    }
}