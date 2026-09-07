using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    [SerializeField] private string itemName = "Item";
    [SerializeField] private Sprite icon;

    public void Collect(Inventory inventory)
    {
        if (inventory.AddItem(itemName, icon))
        {
            // Swap for Destroy(gameObject) once items shouldn't ever reappear.
            gameObject.SetActive(false);
        }
    }
}