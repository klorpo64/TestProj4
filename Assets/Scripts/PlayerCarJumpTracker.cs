using UnityEngine;

public class PlayerCarJumpTracker : MonoBehaviour
{
    public int carsJumped = 0;
    public GameObject popupPrefab;        // Your popup prefab
    public Transform popupSpawnPoint;     // Empty Transform above player's head
    public int requiredJumps = 5;

    private bool coconutAwarded = false;

    public void RegisterCarJump()
    {
        carsJumped++;

        if (popupPrefab != null && popupSpawnPoint != null)
        {
            GameObject popup = Instantiate(popupPrefab, popupSpawnPoint.position, Quaternion.identity);
            popup.GetComponent<JumpCounterPopup>().Initialize(carsJumped);
        }

        if (!coconutAwarded && carsJumped >= requiredJumps)
        {
            coconutAwarded = true;
            GameManager.Instance.SpawnGoldenCoconut();
        }
    }
}
