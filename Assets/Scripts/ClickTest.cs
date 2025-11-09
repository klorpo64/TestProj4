using UnityEngine;
using UnityEngine.UI;  // Needed for Button

public class ClickTest : MonoBehaviour
{
    void Start()
    {
        // Get the Button component on this GameObject
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            // Add a listener that runs when the button is clicked
            btn.onClick.AddListener(OnButtonClicked);
        }
        else
        {
            Debug.LogWarning("No Button component found on this object!");
        }
    }

    void OnButtonClicked()
    {
        Debug.Log(" Button clicked!");
    }
}
