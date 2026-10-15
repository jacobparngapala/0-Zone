using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapeSceneScript : MonoBehaviour
{
    public void NextLevel()
    {
        string lastLevel = PlayerPrefs.GetString("LastLevel", "MainScene");

        switch (lastLevel)
        {
            case "MainScene":
                SceneManager.LoadScene("level2");
                break;
            case "level2":
                SceneManager.LoadScene("level3");
                break;
            default:
                // Finished the last level, nothing after it yet
                SceneManager.LoadScene("MainMenuScene");
                break;
        }
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}