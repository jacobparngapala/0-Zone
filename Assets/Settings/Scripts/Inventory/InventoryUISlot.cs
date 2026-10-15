using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUISlot : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text keyLabel;
    [SerializeField] private TMP_Text nameLabel;

    [SerializeField] private Color normalColor = new Color(0f, 0f, 0f, 0.55f);
    [SerializeField] private Color selectedColor = new Color(1f, 0.85f, 0.2f, 0.9f);
    [SerializeField] private float selectedScale = 1.15f;

    public void Set(ItemData item, int slotNumber, bool selected)
    {
        keyLabel.text = slotNumber.ToString();
        nameLabel.text = item != null ? item.itemName : "";

        icon.sprite = item != null ? item.icon : null;
        icon.enabled = icon.sprite != null;
        icon.color = selected ? Color.white : new Color(1f, 1f, 1f, 0.6f);

        background.color = selected ? selectedColor : normalColor;
        transform.localScale = Vector3.one * (selected ? selectedScale : 1f);
    }
}