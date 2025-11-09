using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    // Key to switch scenes
    public KeyCode switchKey = KeyCode.Space;

    // Name of the scene to load
    public string sceneToLoad = "Walk";

    void Update()
    {
        if (Input.GetKeyDown(switchKey))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}