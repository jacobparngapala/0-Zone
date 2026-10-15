
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level2Script : MonoBehaviour
{


    public void CompleteLevel2()
    {
        // Unlock Level 3
        PlayerPrefs.SetInt("level3", 1);

        // Set the destination to Level 3
        PlayerPrefs.SetString("NextLevelScene", "level3");

        // Save progression
        PlayerPrefs.Save();

        // Verify that Level 3 is unlocked
        Debug.Log("Level 3 unlock value: " +
            PlayerPrefs.GetInt("level3", 0));

        Debug.Log("Next destination: " +
            PlayerPrefs.GetString("NextLevelScene"));

        // Open the shared win screen
        SceneManager.LoadScene("EscapeScene");
    }
}
