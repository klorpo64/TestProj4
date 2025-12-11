using UnityEngine;

public class CoconutSpawnPoint : MonoBehaviour
{
    [Header("Unique Golden Coconut ID for this location")]
    public string coconutID;

    [Header("Cutscene Offset (optional)")]
    public Vector3 cutsceneOffset = new Vector3(0f, 2f, -3f);
}