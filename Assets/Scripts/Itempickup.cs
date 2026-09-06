using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float interactionRange = 3f;

    private Inventory inventory;

    void Awake()
    {
        inventory = GetComponent<Inventory>();
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
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, interactionRange))
        {
            if (hit.collider.TryGetComponent(out InteractableItem item))
            {
                item.Collect(inventory);
            }
        }
    }
}