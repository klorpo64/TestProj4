using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // Define the super tiny scale used for initial challenge spawn
    private const float TINY_SCALE = 0.0000000000001f;
    // Define the regular (final) scale used for scene-loaded coconuts
    private const float REGULAR_SCALE = 0.01f;

    [System.Serializable]
    public struct PersistentCoconut
    {
        public string id;           // Matches CoconutSpawnPoint.coconutID
        public string spawnPointName;
        public bool isChallengeSpawn;

        public Vector3 cutsceneOffset;
    }

    [Header("Golden Coconut Settings")]
    public GameObject goldenCoconutPrefab;
    public GameObject collectedCoconutPrefab;

    [Header("Persistent Coconuts")]
    public PersistentCoconut[] persistentCoconuts;

    [Header("Cutscene Settings")]
    public float cutsceneWaitDuration = 1.5f;
    public float cameraPanDuration = 1f;
    public float cutsceneDownwardPitch = 5f;
    public AudioClip coconutSpawnSound;
    public AudioClip coconutCollectSound;

    private HashSet<string> collectedCoconutIDs = new HashSet<string>();
    private HashSet<string> spawnedCoconutIDsInScene = new HashSet<string>();

    public int CollectedCoconutCount => collectedCoconutIDs.Count;

    [Header("Game State")]
    public bool gameplayFrozen = false;
    private Transform playerTransform;
    private CameraOrbitController cameraController;
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        RefreshCameraController();
        SceneManager.sceneLoaded += OnSceneLoaded;
        FindPlayerInScene();
        gameplayFrozen = false;

        SpawnAllPersistentCoconuts();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        spawnedCoconutIDsInScene.Clear();
        RefreshCameraController();
        FindPlayerInScene();
        gameplayFrozen = false;

        SpawnAllPersistentCoconuts();
    }

    private void FindPlayerInScene()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;
        else playerTransform = null;
    }

    private void RefreshCameraController()
    {
        if (Camera.main == null) return;
        if (cameraController == null || cameraController.gameObject == null)
            cameraController = Camera.main.GetComponent<CameraOrbitController>();
    }

    public bool IsCoconutCollected(string coconutID)
    {
        return collectedCoconutIDs.Contains(coconutID);
    }

    private CoconutSpawnPoint FindSpawnPoint(string id)
    {
        CoconutSpawnPoint[] all = Object.FindObjectsByType<CoconutSpawnPoint>(FindObjectsSortMode.None);

        foreach (var sp in all)
        {
            if (sp.coconutID == id)
                return sp;
        }
        return null;
    }

    private PersistentCoconut? GetPersistentCoconutData(string coconutID)
    {
        foreach (var coconut in persistentCoconuts)
        {
            if (coconut.id == coconutID) return coconut;
        }
        return null;
    }

    // -----------------------------
    // Spawn a coconut at a location (used ONLY for persistent/scene loading)
    // -----------------------------
    public void SpawnCoconutAtLocation(string coconutID, Vector3 spawnPosition, bool playCutscene)
    {
        if (spawnedCoconutIDsInScene.Contains(coconutID)) return;

        bool alreadyCollected = IsCoconutCollected(coconutID);
        GameObject prefabToSpawn = alreadyCollected ? collectedCoconutPrefab : goldenCoconutPrefab;

        if (prefabToSpawn == null)
        {
            Debug.LogError("Coconut prefab not assigned!");
            return;
        }

        GameObject coconut = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);

        // Scene-Loaded coconuts spawn at the final 0.01 scale
        coconut.transform.localScale = new Vector3(REGULAR_SCALE, REGULAR_SCALE, REGULAR_SCALE);

        spawnedCoconutIDsInScene.Add(coconutID);

        GoldenCoconutController gcc = coconut.GetComponent<GoldenCoconutController>();
        if (gcc == null)
        {
            Debug.LogError($"Coconut {coconutID} spawned but is missing GoldenCoconutController!");
            return;
        }

        gcc.Initialize(coconutID, alreadyCollected, false);

        if (!alreadyCollected)
        {
            gcc.StartIdleHover();
        }
    }

    // -----------------------------
    // Spawn a challenge coconut (3 Arguments Standard)
    // -----------------------------
    public void SpawnGoldenCoconut(string coconutID, Transform spawnTransform, bool isChallengeSpawn)
    {
        bool alreadyCollected = IsCoconutCollected(coconutID);
        if (alreadyCollected)
        {
            if (!spawnedCoconutIDsInScene.Contains(coconutID))
            {
                SpawnCoconut(coconutID, spawnTransform.position, true, alreadyCollected, isChallengeSpawn);
            }
            return;
        }

        if (spawnedCoconutIDsInScene.Contains(coconutID))
        {
            Debug.Log("Coconut " + coconutID + " already spawned in this scene and uncollected.");
            return;
        }

        if (isChallengeSpawn && playerTransform != null)
        {
            Transform coconutTransform = SpawnCoconut(coconutID, spawnTransform.position, false, alreadyCollected, isChallengeSpawn);
            if (coconutTransform == null) return;

            PersistentCoconut? pc = GetPersistentCoconutData(coconutID);

            Vector3 targetPosition = coconutTransform.position;
            Vector3 offset = pc?.cutsceneOffset ?? new Vector3(0f, 2f, -3f);

            StartCoroutine(CoconutSpawnCutscene(coconutTransform, playerTransform.position, targetPosition, offset));
        }
        else
        {
            SpawnCoconut(coconutID, spawnTransform.position, true, alreadyCollected, isChallengeSpawn);
        }
    }

    // -----------------------------
    // Helper: Consolidated Coconut Instantiation Logic 
    // -----------------------------
    private Transform SpawnCoconut(string coconutID, Vector3 spawnPosition, bool startIdle, bool isCollected, bool isChallengeSpawn)
    {
        GameObject prefabToSpawn = isCollected ? collectedCoconutPrefab : goldenCoconutPrefab;

        if (prefabToSpawn == null)
        {
            Debug.LogError("Coconut prefab not assigned!");
            return null;
        }

        GameObject coconut = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);

        // Set scale based on type
        if (isChallengeSpawn)
        {
            coconut.transform.localScale = new Vector3(TINY_SCALE, TINY_SCALE, TINY_SCALE);
        }
        else
        {
            // Default to regular scale if not a challenge spawn
            coconut.transform.localScale = new Vector3(REGULAR_SCALE, REGULAR_SCALE, REGULAR_SCALE);
        }

        spawnedCoconutIDsInScene.Add(coconutID);

        GoldenCoconutController gcc = coconut.GetComponent<GoldenCoconutController>();
        if (gcc == null)
        {
            Debug.LogError($"Coconut {coconutID} spawned but is missing GoldenCoconutController!");
            Destroy(coconut);
            return null;
        }

        gcc.Initialize(coconutID, isCollected, prefabToSpawn == collectedCoconutPrefab);

        if (startIdle)
        {
            gcc.StartIdleHover();
        }

        return coconut.transform;
    }

    // -----------------------------
    // Overload for compatibility (if older scripts pass 4 args)
    // -----------------------------
    public void SpawnGoldenCoconut(string coconutID, Transform spawnTransform, bool isChallengeSpawn, bool unusedArgument)
    {
        SpawnGoldenCoconut(coconutID, spawnTransform, isChallengeSpawn);
    }

    // -----------------------------
    // Coconut spawn cutscene coroutine (SMOOTH PAN LOGIC)
    // -----------------------------
    private IEnumerator CoconutSpawnCutscene(Transform coconutTransform, Vector3 playerPosition, Vector3 targetPosition, Vector3 offset)
    {
        yield return null;

        GoldenCoconutController gcc = coconutTransform.GetComponent<GoldenCoconutController>();

        if (cameraController == null || playerTransform == null || coconutTransform == null)
        {
            Debug.LogError("Cutscene dependencies missing! Aborting cutscene.");
            if (gcc != null) gcc.StartIdleHover();
            gameplayFrozen = false;
            yield break;
        }

        gameplayFrozen = true;
        cameraController.LockCamera(true);

        // --- CALCULATIONS ---

        // 1. Player Camera View
        Vector3 playerCamPos = playerTransform.position + cameraController.cameraOffset;
        Quaternion playerCamRot = Quaternion.LookRotation(playerTransform.position - playerCamPos, Vector3.up);

        // 2. Coconut Camera View 
        Vector3 coconutCamPos = targetPosition + offset;
        Quaternion coconutLookRot = Quaternion.LookRotation(targetPosition - coconutCamPos, Vector3.up);
        Quaternion downwardTilt = Quaternion.Euler(cutsceneDownwardPitch, 0, 0);
        Quaternion coconutCamRot = coconutLookRot * downwardTilt;

        // 3. Current Camera State 
        Vector3 startPos = cameraController.transform.position;
        Quaternion startRot = cameraController.transform.rotation;

        // --- STAGE 1: SMOOTH PAN TO COCONUT ---
        float timer = 0f;
        while (timer < cameraPanDuration)
        {
            timer += Time.deltaTime;
            float t = timer / cameraPanDuration;
            cameraController.transform.position = Vector3.Lerp(startPos, coconutCamPos, t);
            cameraController.transform.rotation = Quaternion.Slerp(startRot, coconutCamRot, t);
            yield return null;
        }
        cameraController.transform.position = coconutCamPos;
        cameraController.transform.rotation = coconutCamRot;

        // --- STAGE 2: WAIT AND SPAWN ANIMATION ---
        if (coconutSpawnSound != null && audioSource != null)
            audioSource.PlayOneShot(coconutSpawnSound);

        if (gcc != null)
        {
            gcc.StartSpawnAnimation();
            yield return new WaitForSeconds(cutsceneWaitDuration + 0.5f);
        }

        // --- STAGE 3: SMOOTH PAN BACK TO PLAYER ---
        startPos = cameraController.transform.position;
        startRot = cameraController.transform.rotation;

        timer = 0f;
        while (timer < cameraPanDuration)
        {
            timer += Time.deltaTime;
            float t = timer / cameraPanDuration;
            cameraController.transform.position = Vector3.Lerp(startPos, playerCamPos, t);
            cameraController.transform.rotation = Quaternion.Slerp(startRot, playerCamRot, t);
            yield return null;
        }

        cameraController.transform.position = playerCamPos;
        cameraController.transform.rotation = playerCamRot;

        cameraController.LockCamera(false);
        gameplayFrozen = false;

        if (gcc != null)
            gcc.StartIdleHover();
    }

    // -----------------------------
    // Coconut collection
    // -----------------------------
    public void CollectCoconut(string coconutID)
    {
        if (!collectedCoconutIDs.Contains(coconutID))
        {
            collectedCoconutIDs.Add(coconutID);
            IncrementGoldenCoconutCount();

            if (coconutCollectSound != null && audioSource != null)
                audioSource.PlayOneShot(coconutCollectSound);

            Debug.Log($"Coconut {coconutID} collected. Total: {collectedCoconutIDs.Count}");
        }
    }

    public void IncrementGoldenCoconutCount()
    {
        int totalCollected = collectedCoconutIDs.Count;
        CoconutCounterUI counter = FindAnyObjectByType<CoconutCounterUI>();
        if (counter != null)
            counter.UpdateCount(totalCollected);
    }

    // -----------------------------
    // Only spawn persistent coconuts that are NOT challenge spawns
    // -----------------------------
    private void SpawnAllPersistentCoconuts()
    {
        foreach (var coconut in persistentCoconuts)
        {
            if (coconut.isChallengeSpawn)
                continue;

            CoconutSpawnPoint sp = FindSpawnPoint(coconut.id);
            if (sp == null)
            {
                Debug.LogWarning($"No CoconutSpawnPoint with ID '{coconut.id}' found in scene.");
                continue;
            }

            SpawnCoconutAtLocation(coconut.id, sp.transform.position, false);
        }
    }

    public List<string> CollectedCoconutIDsInScene(string sceneName)
    {
        List<string> list = new List<string>();
        foreach (var coconut in persistentCoconuts)
        {
            if (coconut.spawnPointName.StartsWith(sceneName))
                list.Add(coconut.id);
        }
        return list;
    }

}