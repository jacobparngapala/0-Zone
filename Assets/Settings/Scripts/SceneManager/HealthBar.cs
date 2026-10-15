using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage; // an Image set to "Filled" type

    public void UpdateBar(int current, int max)
    {
        fillImage.fillAmount = (float)current / max;
    }
}