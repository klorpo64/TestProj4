using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject pauseMenuUI;

    public static bool IsPaused = false;

    private void Update()
    {
        // Keyboard Esc OR Gamepad Start
        if (Keyboard.current.escapeKey.wasPressedThisFrame ||
            Gamepad.current?.startButton.wasPressedThisFrame == true)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (IsPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        if (IsPaused) return;

        // UI
        pauseMenuUI.SetActive(true);

        // Stop time
        Time.timeScale = 0f;

        // Stop player/cars/etc
        if (GameManager.Instance != null)
            GameManager.Instance.gameplayFrozen = true;

        IsPaused = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        if (!IsPaused) return;

        pauseMenuUI.SetActive(false);

        // Resume time
        Time.timeScale = 1f;

        // Resume gameplay
        if (GameManager.Instance != null)
            GameManager.Instance.gameplayFrozen = false;

        IsPaused = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}