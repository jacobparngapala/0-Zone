using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Tooltip("Element 0 = key 1, Element 1 = key 2, ...")]
    [SerializeField] private List<ItemData> items = new List<ItemData>();
    [SerializeField] private int startIndex = 0;
    [SerializeField] private AudioClip switchSound;
    [SerializeField, Range(0f, 1f)] private float switchVolume = 0.6f;

    public IReadOnlyList<ItemData> Items => items;
    public int SelectedIndex { get; private set; } = -1;
    public ItemData CurrentItem =>
        (SelectedIndex >= 0 && SelectedIndex < items.Count) ? items[SelectedIndex] : null;

    // (selected slot index, item in that slot)
    public event Action<int, ItemData> OnSelectionChanged;

    private AudioSource sfx;

    private void Awake()
    {
        GameObject go = new GameObject("InventorySFX");
        go.transform.SetParent(transform, false);
        sfx = go.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        sfx.spatialBlend = 0f;
    }

    private void Start()
    {
        // Fires the first event so every listener starts in the right state
        Select(startIndex, false);
    }

    private void Update()
    {
        for (int i = 0; i < items.Count && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
            {
                Select(i, true);
                break;
            }
        }
    }

    public bool IsHolding(ItemType type)
    {
        return CurrentItem != null && CurrentItem.type == type;
    }

    public void Select(int index, bool playSound)
    {
        if (index < 0 || index >= items.Count) return;
        if (playSound && index == SelectedIndex) return;   // same key again does nothing

        SelectedIndex = index;
        if (playSound && switchSound != null) sfx.PlayOneShot(switchSound, switchVolume);
        OnSelectionChanged?.Invoke(SelectedIndex, CurrentItem);
    }
}