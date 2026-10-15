using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit3 : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SceneManager.LoadScene("level3"); // Lilipat sa level3
        }
    }
}