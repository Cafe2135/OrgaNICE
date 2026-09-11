using System;
using UnityEngine;

public class InventorySelector : MonoBehaviour
{
    private Inventory inventory;
    private PlayerInteraction playerInteraction;

    public int SelectedIndex { get; private set; } = -1;
    public event Action OnSelectionChanged;

    void Awake()
    {
        inventory = GetComponent<Inventory>();
        playerInteraction = GetComponent<PlayerInteraction>();
    }

    void Update()
    {
        bool holdingObject = playerInteraction != null && playerInteraction.IsHolding;

        if (!holdingObject)
        {
            for (int i = 0; i < 5; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SelectedIndex = i;
                    LogSelectedTag();
                    OnSelectionChanged?.Invoke();
                }
            }

            if (Input.GetKeyDown(KeyCode.G) && SelectedIndex >= 0 && inventory != null)
            {
                if (SelectedIndex < inventory.Items.Count)
                {
                    GameObject itemObj = inventory.Items[SelectedIndex].sourceObject;
                    
                    Vector3 safeDropPos = playerInteraction != null 
                        ? playerInteraction.CalculateCrosshairDropPosition(itemObj) 
                        : transform.position + transform.forward * 1.5f;

                    inventory.DropAt(SelectedIndex, safeDropPos);
                }
            }
        }
    }

    public ItemTag? GetSelectedTag()
    {
        if (inventory == null || SelectedIndex < 0 || SelectedIndex >= inventory.Items.Count)
        {
            return null;
        }
        return inventory.Items[SelectedIndex].tag;
    }

    private void LogSelectedTag()
    {
        var tag = GetSelectedTag();
        if (tag.HasValue)
        {
            Debug.Log($"Slot {SelectedIndex + 1} selected: {inventory.Items[SelectedIndex].itemName} — {tag.Value}");
        }
    }
}