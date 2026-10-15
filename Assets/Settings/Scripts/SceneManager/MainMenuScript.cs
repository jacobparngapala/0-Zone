using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string LevelsMenu = "LevelsMenu";
    public void LoadLevelSelection()
    {
        SceneManager.LoadScene("LevelSelection");
   
    }
}
