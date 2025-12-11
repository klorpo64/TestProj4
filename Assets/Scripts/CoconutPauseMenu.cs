using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class CoconutPauseMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenuPanel;            // Panel to hold the menu
    public Transform coconutContainer;           // Parent for icons
    public GameObject coconutIconPrefab;         // Prefab for each coconut icon
    public TextMeshProUGUI sceneNameText;
    public TextMeshProUGUI pageText;

    [Header("Navigation")]
    public int iconsPerPage = 8;

    private List<GameManager.PersistentCoconut> sceneCoconuts = new List<GameManager.PersistentCoconut>();
    private List<string> sceneNames = new List<string>();
    private int currentSceneIndex = 0;
    private int currentPage = 0;

    private PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Next.performed += ctx => NextScene();
        controls.Player.Previous.performed += ctx => PreviousScene();
    }

    private void OnEnable()
    {
        controls.Enable();
        RefreshSceneList();
        ShowScene(currentSceneIndex);
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void RefreshSceneList()
    {
        sceneNames.Clear();
        foreach (var coconut in GameManager.Instance.persistentCoconuts)
        {
            string sceneName = coconut.spawnPointName.Split('_')[0];
            if (!sceneNames.Contains(sceneName))
                sceneNames.Add(sceneName);
        }
    }

    private void ShowScene(int sceneIdx)
    {
        if (sceneNames.Count == 0) return;
        currentSceneIndex = Mathf.Clamp(sceneIdx, 0, sceneNames.Count - 1);
        string sceneName = sceneNames[currentSceneIndex];

        sceneNameText.text = sceneName;
        sceneCoconuts.Clear();

        foreach (var coconut in GameManager.Instance.persistentCoconuts)
        {
            if (coconut.spawnPointName.StartsWith(sceneName))
                sceneCoconuts.Add(coconut);
        }

        currentPage = 0;
        PopulatePage();
    }

    private void PopulatePage()
    {
        // Clear previous icons
        foreach (Transform child in coconutContainer)
            Destroy(child.gameObject);

        int startIdx = currentPage * iconsPerPage;
        int endIdx = Mathf.Min(startIdx + iconsPerPage, sceneCoconuts.Count);

        for (int i = startIdx; i < endIdx; i++)
        {
            var coconut = sceneCoconuts[i];
            GameObject iconGO = Instantiate(coconutIconPrefab, coconutContainer);

            Image img = iconGO.GetComponent<Image>();
            TextMeshProUGUI text = iconGO.GetComponentInChildren<TextMeshProUGUI>();

            text.text = coconut.id;
            bool collected = GameManager.Instance.IsCoconutCollected(coconut.id);

            // Fill or empty circle
            img.color = collected ? Color.yellow : Color.gray;
        }

        int totalPages = Mathf.CeilToInt(sceneCoconuts.Count / (float)iconsPerPage);
        pageText.text = $"{currentPage + 1}/{Mathf.Max(totalPages, 1)}";
    }

    private void NextScene()
    {
        if (sceneNames.Count == 0) return;
        currentSceneIndex = (currentSceneIndex + 1) % sceneNames.Count;
        ShowScene(currentSceneIndex);
    }

    private void PreviousScene()
    {
        if (sceneNames.Count == 0) return;
        currentSceneIndex--;
        if (currentSceneIndex < 0) currentSceneIndex = sceneNames.Count - 1;
        ShowScene(currentSceneIndex);
    }
}