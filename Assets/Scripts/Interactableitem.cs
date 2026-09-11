using UnityEngine;

public enum ItemTag
{
    Trash,
    Neutral,
    Needed
}

public class InteractableItem : MonoBehaviour
{
    [SerializeField] private string itemName = "Item";
    [SerializeField] private Sprite icon;
    [SerializeField] private ItemTag itemTag = ItemTag.Neutral;
    [SerializeField, TextArea] private string description = "An item.";

    public ItemTag Tag => itemTag;

    public void Collect(Inventory inventory)
    {
        if (inventory.AddItem(itemName, description, icon, itemTag, gameObject))
        {
            gameObject.SetActive(false);
        }
    }
}