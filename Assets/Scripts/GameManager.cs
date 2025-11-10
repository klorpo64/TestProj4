using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("NPC States")]
    public bool friendNPCTalkedTo = false;

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

    public void SetFriendTalkedTo()
    {
        friendNPCTalkedTo = true;
        Debug.Log("Friend NPC has been talked to.");
    }
}
