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
        public ItemTag tag;
        public GameObject sourceObject;
    }

    private readonly List<Entry> items = new List<Entry>();

    public IReadOnlyList<Entry> Items => items;
    public int MaxSlots => maxSlots;
    public event Action OnInventoryChanged;
    public event Action<string> OnItemPickedUp;

    public bool AddItem(string itemName, string description, Sprite icon, ItemTag tag, GameObject sourceObject)
    {
        if (items.Count >= maxSlots)
        {
            Debug.Log("Inventory full — can't pick up " + itemName);
            return false;
        }

        items.Add(new Entry { itemName = itemName, icon = icon, tag = tag, sourceObject = sourceObject });
        Debug.Log($"Picked up: {itemName} [{tag}] ({items.Count}/{maxSlots})");
        OnInventoryChanged?.Invoke();
        OnItemPickedUp?.Invoke(description);
        return true;
    }
    public bool DropAt(int index, Vector3 position)
    {
        if (index < 0 || index >= items.Count) return false;

        Entry entry = items[index];
        items.RemoveAt(index);

        if (entry.sourceObject != null)
        {
            entry.sourceObject.transform.position = position;
            entry.sourceObject.SetActive(true);

            if (entry.sourceObject.TryGetComponent(out Rigidbody rb))
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        Debug.Log($"Dropped: {entry.itemName} ({items.Count}/{maxSlots})");
        OnInventoryChanged?.Invoke();
        return true;
    }
}