using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public string gameOverScene = "GameOverScene";
    public string mainScene = "MainScene";
    public string mainMenuScene = "MainMenuScene";

    public void ShowGameOver()
    {
        Time.timeScale = 0f;
        SceneManager.LoadScene(gameOverScene);
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainScene);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

}
