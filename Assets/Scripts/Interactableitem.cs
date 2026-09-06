using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    [SerializeField] private string itemName = "Item";

    public void Collect(Inventory inventory)
    {
        inventory.AddItem(itemName);

        // Swap for Destroy(gameObject) once items shouldn't ever reappear.
        gameObject.SetActive(false);
    }
}