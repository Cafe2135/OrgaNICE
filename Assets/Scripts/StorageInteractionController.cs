using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StorageInteractionController : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Inventory inventory;
    [SerializeField] private InventorySelector inventorySelector;
    [SerializeField] private float interactRange = 4f;
    [SerializeField] private LayerMask storageLayer = ~0;

    private SmallStorage activeStorage;
    private bool isInStorageView = false;
    private bool justExitedThisFrame = false;

    private Vector3 originalCamPos;
    private Quaternion originalCamRot;

    private InteractableItem draggedItem;
    private int originInventoryIndex = -1;
    private Transform originSlotTransform;
    private float currentDragYRotation = 0f;
    private enum DragSource { None, Inventory, Storage }
    private DragSource currentDragSource = DragSource.None;

    public static bool IsInStorageMode { get; private set; } = false;
    public static bool JustExitedStorage { get; private set; } = false;

    void Awake()
    {
        if (inventory == null) inventory = GetComponent<Inventory>();
        if (inventorySelector == null) inventorySelector = GetComponent<InventorySelector>();
        if (playerCamera == null) playerCamera = Camera.main;
    }

    void Update()
    {
        if (justExitedThisFrame)
        {
            justExitedThisFrame = false;
            JustExitedStorage = false;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isInStorageView)
            {
                ExitStorageView();
                return;
            }
            else if (!JustExitedStorage)
            {
                TryEnterStorageView();
            }
        }

        if (isInStorageView) HandleStorageDragging();
    }

    private void TryEnterStorageView()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, storageLayer, QueryTriggerInteraction.Collide))
        {
            SmallStorage storage = hit.collider.GetComponentInParent<SmallStorage>();
            if (storage != null && storage.CameraAnchor != null) EnterStorageView(storage);
        }
    }

    private void EnterStorageView(SmallStorage storage)
    {
        activeStorage = storage;
        isInStorageView = true;
        IsInStorageMode = true;

        originalCamPos = playerCamera.transform.position;
        originalCamRot = playerCamera.transform.rotation;

        playerCamera.transform.position = storage.CameraAnchor.position;
        playerCamera.transform.rotation = storage.CameraAnchor.rotation;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        UpdateInventoryVisuals(true);
    }

    private void ExitStorageView()
    {
        if (draggedItem != null) CancelDrag();

        isInStorageView = false;
        IsInStorageMode = false;
        justExitedThisFrame = true;
        JustExitedStorage = true;

        playerCamera.transform.position = originalCamPos;
        playerCamera.transform.rotation = originalCamRot;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        UpdateInventoryVisuals(false);
        activeStorage = null;
    }

    private void HandleStorageDragging()
    {
        if (Input.GetMouseButtonDown(0) && draggedItem == null)
        {
            int clickedSlot = GetClickedInventorySlotIndex();
            if (clickedSlot != -1 && clickedSlot < inventory.Items.Count)
            {
                var itemData = inventory.Items[clickedSlot];
                if (itemData.sourceObject != null && itemData.sourceObject.TryGetComponent(out InteractableItem item))
                {
                    if (item.Size == ItemSize.Small) StartDragFromInventory(clickedSlot, item);
                }
            }
            else
            {
                Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 10f))
                {
                    if (hit.collider.TryGetComponent(out InteractableItem item) && item.Size == ItemSize.Small)
                    {
                        Transform slot = activeStorage.GetSlotOfItem(item);
                        if (slot != null) StartDragFromStorage(slot, item);
                    }
                }
            }
        }

        if (draggedItem != null)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                currentDragYRotation += 90f;
                if (currentDragYRotation >= 360f) currentDragYRotation -= 360f;
            }

            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(-activeStorage.CameraAnchor.forward, activeStorage.transform.position + Vector3.up * 0.1f);
            if (plane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                draggedItem.transform.position = hitPoint;

                Quaternion baseFlatRot = draggedItem.GetFlatBaseRotation(activeStorage.transform);
                draggedItem.transform.rotation = Quaternion.AngleAxis(currentDragYRotation, activeStorage.transform.up) * baseFlatRot;
            }

            if (Input.GetMouseButtonUp(0)) EndDrag();
        }
    }

    private void StartDragFromInventory(int slotIndex, InteractableItem item)
    {
        currentDragSource = DragSource.Inventory;
        originInventoryIndex = slotIndex;
        currentDragYRotation = 0f;

        GameObject obj = inventory.RemoveItemAt(slotIndex);
        draggedItem = obj.GetComponent<InteractableItem>();
        draggedItem.gameObject.SetActive(true);

        if (draggedItem.TryGetComponent(out Collider col)) col.enabled = false;
        
        draggedItem.UnlockFromStorage();
        if (draggedItem.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;

        UpdateInventoryVisuals(true);
    }

    private void StartDragFromStorage(Transform slot, InteractableItem item)
    {
        currentDragSource = DragSource.Storage;
        originSlotTransform = slot;
        currentDragYRotation = item.transform.localEulerAngles.y;

        activeStorage.RemoveItem(item);
        draggedItem = item;

        if (draggedItem.TryGetComponent(out Collider col)) col.enabled = false;
        if (draggedItem.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
    }

    private void EndDrag()
    {
        bool droppedInUI = IsPointerOverInventoryUI();

        if (droppedInUI)
        {
            if (inventory.Items.Count < 5)
            {
                draggedItem.Collect(inventory);
                ClearDragState();
                UpdateInventoryVisuals(true);
                return;
            }
        }
        else
        {
            Transform targetSlot = activeStorage.GetClosestFreeSlot(draggedItem.transform.position, 1.2f);
            if (targetSlot != null)
            {
                activeStorage.PlaceItemInSlot(draggedItem, targetSlot, currentDragYRotation);
                ClearDragState();
                UpdateInventoryVisuals(true);
                return;
            }
        }

        CancelDrag();
    }

    private void CancelDrag()
    {
        if (draggedItem == null) return;

        if (currentDragSource == DragSource.Inventory)
        {
            inventory.AddItem(draggedItem.ItemName, "", draggedItem.Icon, draggedItem.Tag, draggedItem.gameObject);
            draggedItem.UnlockFromStorage();
            draggedItem.gameObject.SetActive(false);
        }
        else if (currentDragSource == DragSource.Storage && originSlotTransform != null)
        {
            activeStorage.PlaceItemInSlot(draggedItem, originSlotTransform, currentDragYRotation);
        }

        ClearDragState();
        UpdateInventoryVisuals(true);
    }

    private void ClearDragState()
    {
        draggedItem = null;
        originInventoryIndex = -1;
        originSlotTransform = null;
        currentDragSource = DragSource.None;
    }

    private void UpdateInventoryVisuals(bool storageActive)
    {
        Image[] allImages = FindObjectsByType<Image>(FindObjectsSortMode.None);
        List<Image> iconImages = new List<Image>();

        foreach (var img in allImages)
        {
            if (img.gameObject.name.Contains("Icon") || img.gameObject.name.Contains("Item") || img.gameObject.name.Contains("Slot"))
            {
                iconImages.Add(img);
            }
        }

        for (int i = 0; i < inventory.Items.Count; i++)
        {
            var itemData = inventory.Items[i];
            if (itemData.sourceObject != null && itemData.sourceObject.TryGetComponent(out InteractableItem item))
            {
                bool isLarge = (item.Size == ItemSize.Large);
                Color dimColor = (storageActive && isLarge) ? new Color(0.25f, 0.25f, 0.25f, 0.4f) : Color.white;

                foreach (var img in iconImages)
                {
                    if (img.sprite == itemData.icon || img.gameObject.name.EndsWith((i + 1).ToString()))
                    {
                        img.color = dimColor;
                    }
                }
            }
        }
    }

    private int GetClickedInventorySlotIndex()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            string name = result.gameObject.name;
            for (int i = 0; i < 5; i++)
            {
                if (name.Contains((i + 1).ToString())) return i;
            }
        }
        return -1;
    }

    private bool IsPointerOverInventoryUI()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }
}