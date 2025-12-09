using UnityEngine;

public class CarJumpZone : MonoBehaviour
{
    private bool counted = false;

    private void OnTriggerEnter(Collider other)
    {
        if (counted) return;
        if (!other.CompareTag("Player")) return;

        counted = true;

        PlayerCarJumpTracker tracker = other.GetComponent<PlayerCarJumpTracker>();
        if (tracker != null)
        {
            tracker.RegisterCarJump();
        }
    }
}