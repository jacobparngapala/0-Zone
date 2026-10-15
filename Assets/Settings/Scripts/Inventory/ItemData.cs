using UnityEngine;

public enum ItemType {Flashlight, Gun}

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName = "Item";
    public ItemType type;
    public Sprite icon;
}