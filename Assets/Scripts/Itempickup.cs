using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float interactionRange = 3f;

    private Inventory inventory;
    private PlayerInteraction playerInteraction;

    void Awake()
    {
        inventory = GetComponent<Inventory>();
        playerInteraction = GetComponent<PlayerInteraction>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        // 1. If currently holding/floating an item, return it back to inventory
        if (playerInteraction != null && playerInteraction.IsHolding)
        {
            if (playerInteraction.HeldBody.TryGetComponent(out InteractableItem heldItem))
            {
                playerInteraction.ForceRelease();
                heldItem.Collect(inventory);
                return;
            }
        }

        // 2. Otherwise raycast to pick up world object into inventory
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, interactionRange))
        {
            if (hit.collider.TryGetComponent(out InteractableItem item))
            {
                item.Collect(inventory);
            }
        }
    }
}