using UnityEngine;
using UnityEngine.SceneManagement;
using DialogueEditor;

public class DrownedOptionHandler : MonoBehaviour
{
    public void RestartBeach()
    {
        SceneManager.LoadScene("Beach");
    }

    public void ReturnWorkingHouse()
    {
        SceneManager.LoadScene("WorkingHouse");
    }
}
