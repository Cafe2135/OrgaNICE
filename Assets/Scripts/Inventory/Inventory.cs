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

    // Removes item entry from slot and returns the source GameObject for pulling out
    public GameObject RemoveItemAt(int index)
    {
        if (index < 0 || index >= items.Count) return null;

        Entry entry = items[index];
        items.RemoveAt(index);

        Debug.Log($"Pulled out: {entry.itemName} from slot {index + 1}");
        OnInventoryChanged?.Invoke();
        return entry.sourceObject;
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