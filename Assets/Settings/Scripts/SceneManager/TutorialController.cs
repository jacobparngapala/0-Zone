using UnityEngine;

public class TutorialController : MonoBehaviour
{
    [SerializeField] private GameObject backgroundOverlay;

    private bool isTutorialActive = true;
    private float lastTapTime = 0f;
    private float doubleTapThreshold = 0.3f;

    void Start()
    {

        if (backgroundOverlay != null)
        {
            backgroundOverlay.SetActive(true);
        }

        // 2. Pause the game
        Time.timeScale = 0f;
    }

    void Update()
    {
        if (!isTutorialActive) return;


        if (Input.GetMouseButtonDown(0))
        {
            if (Time.unscaledTime - lastTapTime <= doubleTapThreshold)
            {
                DismissTutorial();
            }
            lastTapTime = Time.unscaledTime;
        }
    }

    void DismissTutorial()
    {
        isTutorialActive = false;

        if (backgroundOverlay != null)
        {
            backgroundOverlay.SetActive(false); // Hides the dark overlay and tutorial
        }


        Time.timeScale = 1f;
    }
}
