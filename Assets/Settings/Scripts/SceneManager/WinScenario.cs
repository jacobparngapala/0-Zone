
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinScenario : MonoBehaviour
{
    [SerializeField] private string nextLevelScene = "level2";
    [SerializeField] private string escapeScene = "EscapeScene";

    private bool triggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered)
            return;

        if (other.CompareTag("Player"))
        {
            triggered = true;

            // Save the destination for the Next Level button
            PlayerPrefs.SetString("NextLevelScene", nextLevelScene);
            PlayerPrefs.Save();

            Debug.Log("Player reached escape zone.");
            Debug.Log("Next destination saved: " + nextLevelScene);

            // Load the shared win screen
            SceneManager.LoadScene(escapeScene);
        }
    }
}
