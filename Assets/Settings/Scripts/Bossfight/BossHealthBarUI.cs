using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Boss health bar that lives on the UI Canvas.
// Hidden until the boss spawns. BossController calls Show(), UpdateBar() and Hide().
// UpdateBar(int,int) has the same signature as HealthBarUI.UpdateBar and Health.OnHealthChanged.
public class BossHealthBarUI : MonoBehaviour
{
    [Header("Required")]
    [Tooltip("The whole bar (background + fill + label). Turned on/off by Show/Hide")]
    [SerializeField] private GameObject root;
    [Tooltip("Image with Image Type = Filled, Fill Method = Horizontal")]
    [SerializeField] private Image fillImage;

    [Header("Optional")]
    [Tooltip("Slower, paler bar behind the fill that trails after damage")]
    [SerializeField] private Image trailImage;
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private TMP_Text valueLabel;     // shows "300 / 300"

    [Header("Feel")]
    [SerializeField] private float fillSpeed = 4f;     // how fast the main fill catches up
    [SerializeField] private float trailDelay = 0.4f;  // pause before the trail starts moving
    [SerializeField] private float trailSpeed = 0.5f;  // fill units per second

    private float targetFill = 1f;
    private float trailTimer;

    private void Awake()
    {
        if (root != null && root != gameObject) root.SetActive(false);
        else if (root == gameObject) SetChildrenActive(false);
    }

    public void Show(string bossName, int current, int max)
    {
        SetActiveAll(true);
        if (nameLabel != null) nameLabel.text = bossName;

        targetFill = Mathf.Clamp01((float)current / Mathf.Max(max, 1));
        if (fillImage != null) fillImage.fillAmount = targetFill;
        if (trailImage != null) trailImage.fillAmount = targetFill;
        RefreshValueLabel(current, max);
    }

    public void UpdateBar(int current, int max)
    {
        float newFill = Mathf.Clamp01((float)current / Mathf.Max(max, 1));
        if (newFill < targetFill) trailTimer = trailDelay;
        targetFill = newFill;
        RefreshValueLabel(current, max);
    }

    public void Hide()
    {
        SetActiveAll(false);
    }

    private void Update()
    {
        if (fillImage != null)
            fillImage.fillAmount = Mathf.MoveTowards(fillImage.fillAmount, targetFill, fillSpeed * Time.deltaTime);

        if (trailImage != null)
        {
            if (trailTimer > 0f) trailTimer -= Time.deltaTime;
            else trailImage.fillAmount = Mathf.MoveTowards(trailImage.fillAmount, targetFill, trailSpeed * Time.deltaTime);
        }
    }

    private void RefreshValueLabel(int current, int max)
    {
        if (valueLabel != null) valueLabel.text = Mathf.Max(current, 0) + " / " + max;
    }

    private void SetActiveAll(bool on)
    {
        if (root == null) return;
        if (root == gameObject) SetChildrenActive(on);
        else root.SetActive(on);
    }

    // If the script sits on the same object as the bar, keep this script alive and toggle the children
    private void SetChildrenActive(bool on)
    {
        foreach (Transform child in transform) child.gameObject.SetActive(on);
    }
}
