
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelsMenuScript : MonoBehaviour
{
    public Button Level1Button;
    public Button Level2Button;
    public Button Level3Button;
    public Button MainMenuButton;

    void Start()
    {
        
        Level1Button.interactable = true;

      
        Level2Button.interactable =
            PlayerPrefs.GetInt("level2", 0) == 1;

        
        Level3Button.interactable =
            PlayerPrefs.GetInt("level3", 0) == 1;

        
        MainMenuButton.interactable = true;
    }

    public void LoadLevel1()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void LoadLevel2()
    {
        SceneManager.LoadScene("level2");
    }

    public void LoadLevel3()
    {
        SceneManager.LoadScene("level3");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}
