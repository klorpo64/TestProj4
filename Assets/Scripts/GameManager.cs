using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Golden Coconut")]
    public GameObject goldenCoconutPrefab;
    public string coconutSpawnPointName = "CoconutSpawnPoint"; // name of spawn point in scene

    [Header("Cutscene Settings")]
    public float cutsceneWaitDuration = 1.5f;
    public float cameraPanDuration = 1f;

    [Header("Game State")]
    public bool gameplayFrozen = false; // freeze player during coconut cutscene
    private bool coconutAlreadySpawnedThisScene = false;
    public bool coconutCollected = false;

    private CameraOrbitController cameraController;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        RefreshCameraController();
        coconutAlreadySpawnedThisScene = false;
    }

    /// <summary>
    /// Safe way to get camera controller (even if camera was replaced)
    /// </summary>
    private void RefreshCameraController()
    {
        if (Camera.main == null) return;

        if (cameraController == null || cameraController.gameObject == null)
            cameraController = Camera.main.GetComponent<CameraOrbitController>();
    }

    /// <summary>
    /// Finds the current scene's coconut spawn point
    /// </summary>
    private Transform GetCoconutSpawnPoint()
    {
        GameObject spawnObj = GameObject.Find(coconutSpawnPointName);
        if (spawnObj != null)
            return spawnObj.transform;

        Debug.LogError($"CoconutSpawnPoint '{coconutSpawnPointName}' not found in scene!");
        return null;
    }

    /// <summary>
    /// Call this to spawn the golden coconut
    /// </summary>
    public void SpawnGoldenCoconut()
    {
        if (coconutAlreadySpawnedThisScene)
            return; // only spawn once per scene

        coconutAlreadySpawnedThisScene = true;
        StartCoroutine(CoconutCutscene());
    }

    private IEnumerator CoconutCutscene()
    {
        gameplayFrozen = true; // freeze player input

        RefreshCameraController();
        if (cameraController != null)
            cameraController.LockCamera(true);

        Transform spawnPoint = GetCoconutSpawnPoint();
        if (spawnPoint == null)
        {
            gameplayFrozen = false;
            yield break;
        }

        // Record starting camera position/rotation safely
        Vector3 startPos = cameraController != null ? cameraController.GetCurrentCameraPosition() : Vector3.zero;
        Vector3 startRot = cameraController != null ? cameraController.GetCurrentCameraRotation() : Vector3.zero;

        // Compute focus position
        Vector3 focusPos = spawnPoint.position + (new Vector3(-3f, 3f, -3f).normalized * 5f);
        Quaternion focusRot = Quaternion.LookRotation(spawnPoint.position - focusPos);

        // Pan to coconut
        float t = 0f;
        while (t < cameraPanDuration)
        {
            t += Time.deltaTime;
            float p = t / cameraPanDuration;

            RefreshCameraController();
            if (cameraController != null)
            {
                cameraController.OverridePosition(Vector3.Lerp(startPos, focusPos, p));
                cameraController.OverrideRotation(Quaternion.Slerp(Quaternion.Euler(startRot), focusRot, p));
            }

            yield return null;
        }

        if (cameraController != null)
        {
            cameraController.OverridePosition(focusPos);
            cameraController.OverrideRotation(focusRot);
        }

        // Spawn the coconut safely
        if (goldenCoconutPrefab != null && !coconutCollected)
        {
            Instantiate(goldenCoconutPrefab, spawnPoint.position, Quaternion.identity);
        }

        // Wait so player can see it
        yield return new WaitForSeconds(cutsceneWaitDuration);

        // Pan camera back
        t = 0f;
        while (t < cameraPanDuration)
        {
            t += Time.deltaTime;
            float p = t / cameraPanDuration;

            RefreshCameraController();
            if (cameraController != null)
            {
                Vector3 rot = Vector3.Lerp(focusRot.eulerAngles,
                                           cameraController.GetInitialRotation(),
                                           p);
                cameraController.OverrideRotation(Quaternion.Euler(rot));
            }

            yield return null;
        }

        if (cameraController != null)
            cameraController.LockCamera(false);

        gameplayFrozen = false; // unfreeze player input
    }

    /// <summary>
    /// Increment the golden coconut counter (connect to UI)
    /// </summary>
    public void IncrementGoldenCoconutCount()
    {
        if (!coconutCollected)
        {
            coconutCollected = true; // mark as collected to prevent double count
            Debug.Log("Golden coconut collected!");
        }
    }

    /// <summary>
    /// Call this to reset the per-scene coconut spawn flag (useful when scene reloads)
    /// </summary>
    public void ResetSceneCoconut()
    {
        coconutAlreadySpawnedThisScene = false;
    }
}
