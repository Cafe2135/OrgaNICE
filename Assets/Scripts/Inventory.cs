using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private int maxSlots = 5;

    public struct Entry
    {
        public string itemName;
        public Sprite icon;
    }

    private readonly List<Entry> items = new List<Entry>();

    public IReadOnlyList<Entry> Items => items;
    public int MaxSlots => maxSlots;
    public event Action OnInventoryChanged;

    public bool AddItem(string itemName, Sprite icon)
    {
        if (items.Count >= maxSlots)
        {
            Debug.Log("Inventory full — can't pick up " + itemName);
            return false;
        }

        items.Add(new Entry { itemName = itemName, icon = icon });
        Debug.Log($"Picked up: {itemName} ({items.Count}/{maxSlots})");
        OnInventoryChanged?.Invoke();
        return true;
    }
}