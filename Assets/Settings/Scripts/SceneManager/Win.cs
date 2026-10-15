
using UnityEngine;
using UnityEngine.SceneManagement;

public class Win : MonoBehaviour
{
    public void NextLevel()
    {
        string nextScene = PlayerPrefs.GetString(
            "NextLevelScene", ""
        );

        if (string.IsNullOrEmpty(nextScene))
        {
            Debug.LogError("Next level scene is not set!");
            return;
        }

        Debug.Log("Loading next level: " + nextScene);

        SceneManager.LoadScene(nextScene);
    }
}
