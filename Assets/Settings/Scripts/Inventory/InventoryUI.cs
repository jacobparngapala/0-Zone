using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private InventoryManager inventory;
    [SerializeField] private InventoryUISlot[] slots;

    private void Awake()
    {
        if (inventory == null) inventory = FindFirstObjectByType<InventoryManager>();
    }

    private void OnEnable()
    {
        if (inventory != null) inventory.OnSelectionChanged += Refresh;
    }

    private void OnDisable()
    {
        if (inventory != null) inventory.OnSelectionChanged -= Refresh;
    }

    private void Refresh(int selectedIndex, ItemData current)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            ItemData item = i < inventory.Items.Count ? inventory.Items[i] : null;
            slots[i].Set(item, i + 1, i == selectedIndex);
        }
    }
}