using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit2 : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SceneManager.LoadScene("level2"); // Lilipat sa level2
        }
    }
}
