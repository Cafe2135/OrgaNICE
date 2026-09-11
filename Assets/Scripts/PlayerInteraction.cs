using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Inventory inventory;
    [SerializeField] private InventorySelector inventorySelector;

    [SerializeField] private float inspectDistance = 1.2f;
    [SerializeField] private float followSpeed = 15f;
    [SerializeField] private float mouseRotateSensitivity = 4f;

    [SerializeField] private float maxDropRange = 3.5f;
    [SerializeField] private LayerMask dropSurfaceLayers = ~0;

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

        if (isInspecting && Input.GetKeyDown(KeyCode.G))
        {
            ReleaseAtCrosshair();
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
        if (inventory == null || inventory.Items.Count == 0) return;

        int targetSlot = (inventorySelector != null && inventorySelector.SelectedIndex >= 0) 
            ? inventorySelector.SelectedIndex 
            : 0;

        if (targetSlot >= inventory.Items.Count) return;

        GameObject obj = inventory.RemoveItemAt(targetSlot);
        if (obj != null)
        {
            if (playerCamera == null) return;

            Vector3 targetSpawnPos = playerCamera.transform.position + playerCamera.transform.forward * inspectDistance;
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, inspectDistance, dropSurfaceLayers, QueryTriggerInteraction.Ignore))
            {
                targetSpawnPos = hit.point - playerCamera.transform.forward * 0.2f;
            }

            obj.transform.position = targetSpawnPos;
            obj.SetActive(true);

            if (obj.TryGetComponent(out Rigidbody rb))
            {
                StartHolding(rb);
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

    private void ReleaseAtCrosshair()
    {
        if (heldBody == null) return;

        Vector3 dropPos = CalculateCrosshairDropPosition(heldBody.gameObject);

        heldBody.position = dropPos;
        heldBody.useGravity = true;
        heldBody.linearVelocity = Vector3.zero;
        heldBody.angularVelocity = Vector3.zero;

        heldBody = null;
        isInspecting = false;
        LockCameraLook = false;
    }

    public Vector3 CalculateCrosshairDropPosition(GameObject itemObj)
    {
        Collider[] colliders = itemObj.GetComponentsInChildren<Collider>();
        foreach (var c in colliders) c.enabled = false;

        float itemHalfHeight = 0.2f;
        if (itemObj.TryGetComponent(out Collider col))
        {
            itemHalfHeight = col.bounds.extents.y;
        }

        Vector3 rayOrigin = playerCamera.transform.position + playerCamera.transform.forward * 0.4f;
        Ray ray = new Ray(rayOrigin, playerCamera.transform.forward);
        Vector3 targetPos;

        if (Physics.Raycast(ray, out RaycastHit hit, maxDropRange, dropSurfaceLayers, QueryTriggerInteraction.Ignore))
        {
            Vector3 normalOffset = Vector3.Dot(hit.normal, Vector3.up) > 0.3f ? Vector3.up : hit.normal;
            targetPos = hit.point + normalOffset * (itemHalfHeight + 0.02f);
        }
        else
        {
            targetPos = rayOrigin + playerCamera.transform.forward * maxDropRange;
        }

        foreach (var c in colliders) c.enabled = true;

        return targetPos;
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