
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level1Script : MonoBehaviour
{
    public void CompleteLevel1()
    {
        PlayerPrefs.SetInt("level2", 1);
        PlayerPrefs.SetString("NextLevelScene", "level2");
        PlayerPrefs.Save();

        SceneManager.LoadScene("EscapeScene");
    }

    public void BackToLevelMenu()
    {
        SceneManager.LoadScene("MainScene");
    }
}
