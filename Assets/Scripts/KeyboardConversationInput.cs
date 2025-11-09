using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class KeyboardConversationInput : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Root GameObject for the dialogue text panel (set active when dialogue is showing).")]
    public GameObject panelDialogue;

    [Tooltip("Root GameObject that contains option Buttons as children (set active when choices are shown).")]
    public GameObject panelOptions;

    [Tooltip("Button that advances the dialogue when there are no choices (assign the same button your mouse normally clicks).")]
    public Button continueButton;

    [Header("Navigation")]
    [Tooltip("Wrap selection when reaching first/last option?")]
    public bool wrapNavigation = true;

    private List<Button> optionButtons = new List<Button>();
    private int selectedIndex = 0;

    void Start()
    {
        if (panelOptions != null)
            RefreshOptionButtons();

        // Make sure EventSystem exists
        if (EventSystem.current == null)
        {
            Debug.LogWarning("No EventSystem found in scene. Keyboard navigation requires an EventSystem.");
        }
    }

    void Update()
    {
        if (panelDialogue == null) return;

        // Only respond when dialogue panel is active
        if (!panelDialogue.activeInHierarchy) return;

        bool optionsActive = panelOptions != null && panelOptions.activeInHierarchy;

        // If choices are visible -> navigate choices
        if (optionsActive)
        {
            // keep the optionButtons list current
            RefreshOptionButtons();

            if (optionButtons.Count == 0) return;

            // Up / Down (also support W/S and Left/Right)
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                ChangeSelection(-1);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                ChangeSelection(+1);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                ChangeSelection(-1);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ChangeSelection(+1);
            }

            // Space or Enter to choose
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                ActivateSelectedOption();
            }
        }
        else
        {
            // No choices visible -> Space or Enter should continue
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                if (continueButton != null)
                {
                    continueButton.onClick.Invoke();
                }
                else
                {
                    Debug.LogWarning("KeyboardConversationInput: continueButton not assigned.");
                }
            }
        }
    }

    void RefreshOptionButtons()
    {
        optionButtons.Clear();
        if (panelOptions == null) return;

        // Find Button components in immediate children (or deeper)
        Button[] found = panelOptions.GetComponentsInChildren<Button>(true);
        foreach (var b in found)
        {
            optionButtons.Add(b);
        }

        // Clamp selectedIndex
        if (optionButtons.Count == 0) selectedIndex = 0;
        else selectedIndex = Mathf.Clamp(selectedIndex, 0, optionButtons.Count - 1);

        // Ensure initial selection highlighted
        if (optionButtons.Count > 0)
            Highlight(selectedIndex);
    }

    void ChangeSelection(int delta)
    {
        if (optionButtons.Count == 0) return;

        selectedIndex += delta;
        if (wrapNavigation)
        {
            if (selectedIndex < 0) selectedIndex = optionButtons.Count - 1;
            if (selectedIndex >= optionButtons.Count) selectedIndex = 0;
        }
        else
        {
            selectedIndex = Mathf.Clamp(selectedIndex, 0, optionButtons.Count - 1);
        }

        Highlight(selectedIndex);
    }

    void Highlight(int index)
    {
        if (optionButtons.Count == 0) return;

        Button btn = optionButtons[index];

        // Set the Unity EventSystem selected GameObject so keyboard navigation visuals work
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(btn.gameObject);

        // Optional: visually indicate selection by calling Select()
        btn.Select();
    }

    void ActivateSelectedOption()
    {
        if (optionButtons.Count == 0) return;

        Button sel = optionButtons[selectedIndex];

        // Invoke the Button's onClick (same as clicking with mouse)
        sel.onClick.Invoke();
    }

    // Public helper: call this if the option buttons change dynamically and you want to refresh manually
    public void ForceRefreshOptions()
    {
        RefreshOptionButtons();
    }
}
