
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level2WinScript : MonoBehaviour
{
    public void LoadLevel3()
    {
        SceneManager.LoadScene("level3");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}
