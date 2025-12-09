using UnityEngine;
using TMPro;

public class CoconutCounterUI : MonoBehaviour
{
    public TextMeshProUGUI countText;

    public void UpdateCount(int count)
    {
        if (countText != null)
            countText.text = count.ToString();
    }
}