using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Inventory inventory;
    [SerializeField] private InventorySelector inventorySelector;

    [Header("Inventory Pull-Out & Float")]
    [SerializeField] private float inspectDistance = 1.2f;
    [SerializeField] private float followSpeed = 15f;
    [SerializeField] private float mouseRotateSensitivity = 4f;

    private Rigidbody heldBody;
    private bool isInspecting = false;

    public static bool LockCameraLook { get; private set; } = false;
    public bool IsHolding => heldBody != null;
    public Rigidbody HeldBody => heldBody;

    void Awake()
    {
        if (inventory == null) inventory = GetComponent<Inventory>();
        if (inventorySelector == null) inventorySelector = GetComponent<InventorySelector>();
        if (playerCamera == null) playerCamera = Camera.main;
    }

    void Update()
    {
        // Toggle 'E': Pull out item when not holding, put back into inventory when holding
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isInspecting)
            {
                TryPutBackInInventory();
            }
            else
            {
                TryPullOutFromInventory();
            }
        }

        // Press 'G' to drop currently held item into the world
        if (isInspecting && Input.GetKeyDown(KeyCode.G))
        {
            Release();
        }

        HandleInspectRotation();
    }

    void FixedUpdate()
    {
        if (heldBody != null)
        {
            MoveHeldObject();
        }
    }

    private void TryPullOutFromInventory()
    {
        if (inventory == null)
        {
            Debug.LogError("PlayerInteraction: Inventory reference is missing!");
            return;
        }

        if (inventory.Items.Count == 0)
        {
            Debug.Log("Inventory is empty — nothing to pull out.");
            return;
        }

        int targetSlot = (inventorySelector != null && inventorySelector.SelectedIndex >= 0) 
            ? inventorySelector.SelectedIndex 
            : 0;

        if (targetSlot >= inventory.Items.Count)
        {
            Debug.Log($"No item in inventory slot {targetSlot + 1}. Select a valid slot.");
            return;
        }

        GameObject obj = inventory.RemoveItemAt(targetSlot);
        if (obj != null)
        {
            if (playerCamera == null)
            {
                Debug.LogError("PlayerInteraction: Player Camera is not assigned!");
                return;
            }

            obj.transform.position = playerCamera.transform.position + playerCamera.transform.forward * inspectDistance;
            obj.SetActive(true);

            if (obj.TryGetComponent(out Rigidbody rb))
            {
                StartHolding(rb);
            }
            else
            {
                Debug.LogWarning($"Pulled out {obj.name}, but it is missing a Rigidbody component!");
            }
        }
    }

    private void TryPutBackInInventory()
    {
        if (heldBody == null) return;

        if (heldBody.TryGetComponent(out InteractableItem item))
        {
            Release();
            item.Collect(inventory);
        }
        else
        {
            Debug.LogWarning($"Item {heldBody.name} is missing InteractableItem script — cannot return to inventory!");
        }
    }

    private void StartHolding(Rigidbody body)
    {
        heldBody = body;
        heldBody.useGravity = false;
        heldBody.linearVelocity = Vector3.zero;
        heldBody.angularVelocity = Vector3.zero;
        isInspecting = true;
    }

    public void ForceRelease()
    {
        Release();
    }

    private void Release()
    {
        if (heldBody == null) return;

        heldBody.useGravity = true;
        heldBody.linearVelocity = Vector3.zero;
        heldBody = null;
        isInspecting = false;
        LockCameraLook = false;
    }

    private void MoveHeldObject()
    {
        Vector3 targetPoint = playerCamera.transform.position + playerCamera.transform.forward * inspectDistance;
        Vector3 toTarget = targetPoint - heldBody.position;
        heldBody.linearVelocity = toTarget * followSpeed;
    }

    private void HandleInspectRotation()
    {
        if (isInspecting && heldBody != null)
        {
            if (Input.GetMouseButton(0))
            {
                LockCameraLook = true;

                float mouseX = Input.GetAxis("Mouse X") * mouseRotateSensitivity;
                float mouseY = Input.GetAxis("Mouse Y") * mouseRotateSensitivity;

                heldBody.transform.Rotate(playerCamera.transform.up, -mouseX, Space.World);
                heldBody.transform.Rotate(playerCamera.transform.right, mouseY, Space.World);
            }
            else
            {
                LockCameraLook = false;
            }
        }
        else
        {
            LockCameraLook = false;
        }
    }
}