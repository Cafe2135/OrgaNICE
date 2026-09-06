using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private readonly List<string> items = new List<string>();

    public IReadOnlyList<string> Items => items;

    public void AddItem(string itemName)
    {
        items.Add(itemName);
        Debug.Log($"Picked up: {itemName} (inventory: {items.Count} item(s))");
    }
}