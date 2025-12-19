using UnityEngine;
using TMPro;

public class CoconutCounterUI : MonoBehaviour
{
    public TextMeshProUGUI countText;

    private int currentCount = 0;

    void Start()
    {
        // Initialize display with current count from GameManager
        if (GameManager.Instance != null)
        {
            UpdateCount(GameManager.Instance.CollectedCoconutCount);
        }
    }

    // Call this to update the displayed count
    public void UpdateCount(int totalCollected)
    {
        currentCount = totalCollected;
        if (countText != null)
        {
            countText.text = currentCount.ToString();
        }
    }
}
