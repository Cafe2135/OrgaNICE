using System.Collections;
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

    private SmallStorage activeDrawer;
    private CabinetStorage activeCabinet;
    private bool isInStorageView = false;
    private bool justExitedThisFrame = false;

    private Vector3 originalCamPos;
    private Quaternion originalCamRot;

    private InteractableItem draggedItem;
    private int originInventoryIndex = -1;
    private Transform originSlotTransform;
    private float currentDragYRotation = 0f;
    private enum DragSource { None, Inventory, Drawer, Cabinet }
    private DragSource currentDragSource = DragSource.None;

    public static bool IsInStorageMode { get; private set; } = false;
    public static bool JustExitedStorage { get; private set; } = false;

    private Transform ActiveCameraAnchor => activeDrawer != null ? activeDrawer.CameraAnchor : (activeCabinet != null ? activeCabinet.CameraAnchor : null);

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
            else if (!JustExitedStorage && !ChecklistUI.IsChecklistOpen)
            {
                TryEnterStorageView();
            }
        }

        if (isInStorageView)
        {
            UpdateCameraToAnchor();
            HandleStorageDragging();
        }
    }

    private void TryEnterStorageView()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, storageLayer, QueryTriggerInteraction.Collide))
        {
            SmallStorage drawer = hit.collider.GetComponentInParent<SmallStorage>();
            if (drawer != null && drawer.CameraAnchor != null)
            {
                EnterDrawerStorage(drawer);
                return;
            }

            CabinetStorage cabinet = hit.collider.GetComponentInParent<CabinetStorage>();
            if (cabinet != null && cabinet.CameraAnchor != null)
            {
                EnterCabinetStorage(cabinet);
                return;
            }
        }
    }

    private void EnterDrawerStorage(SmallStorage drawer)
    {
        activeDrawer = drawer;
        activeCabinet = null;
        StartStorageView();
        activeDrawer.OpenDrawer();
    }

    private void EnterCabinetStorage(CabinetStorage cabinet)
    {
        activeCabinet = cabinet;
        activeDrawer = null;
        StartStorageView();
        activeCabinet.OpenCabinet();
    }

    private void StartStorageView()
    {
        isInStorageView = true;
        IsInStorageMode = true;

        originalCamPos = playerCamera.transform.position;
        originalCamRot = playerCamera.transform.rotation;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        UpdateInventoryVisuals(true);
    }

    private void ExitStorageView()
    {
        if (draggedItem != null) CancelDrag();

        if (activeDrawer != null) activeDrawer.CloseDrawer();
        if (activeCabinet != null) activeCabinet.CloseCabinet();

        isInStorageView = false;
        IsInStorageMode = false;
        justExitedThisFrame = true;
        JustExitedStorage = true;

        playerCamera.transform.position = originalCamPos;
        playerCamera.transform.rotation = originalCamRot;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        UpdateInventoryVisuals(false);
        activeDrawer = null;
        activeCabinet = null;
    }

    private void UpdateCameraToAnchor()
    {
        Transform anchor = ActiveCameraAnchor;
        if (anchor != null)
        {
            playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, anchor.position, Time.deltaTime * 10f);
            playerCamera.transform.rotation = Quaternion.Slerp(playerCamera.transform.rotation, anchor.rotation, Time.deltaTime * 10f);
        }
    }

    private void HandleStorageDragging()
    {
        Transform anchor = ActiveCameraAnchor;
        if (anchor == null) return;

        // 1. START DRAGGING
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
                if (Physics.Raycast(ray, out RaycastHit hit, 15f))
                {
                    InteractableItem item = hit.collider.GetComponentInParent<InteractableItem>();
                    if (item != null && item.Size == ItemSize.Small)
                    {
                        if (activeDrawer != null)
                        {
                            Transform slot = activeDrawer.GetSlotOfItem(item);
                            if (slot != null) StartDragFromStorage(slot, item, DragSource.Drawer);
                        }
                        else if (activeCabinet != null)
                        {
                            Transform slot = activeCabinet.GetSlotOfItem(item);
                            if (slot != null) StartDragFromStorage(slot, item, DragSource.Cabinet);
                        }
                    }
                }
            }
        }

        // 2. WHILE DRAGGING (Supports Tilted Camera View)
        if (draggedItem != null)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                currentDragYRotation += 90f;
                if (currentDragYRotation >= 360f) currentDragYRotation -= 360f;
            }

            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

            Vector3 planeNormal = -anchor.forward;
            Vector3 planePosition = anchor.position;

            if (activeCabinet != null)
            {
                // Keeps plane perfectly upright along the cabinet face even when camera tilts down
                planeNormal = -activeCabinet.transform.forward;
                if (activeCabinet.SnapSlots.Count > 0 && activeCabinet.SnapSlots[0] != null)
                {
                    planePosition = activeCabinet.SnapSlots[0].position;
                }
            }
            else if (activeDrawer != null && activeDrawer.SnapSlots.Count > 0 && activeDrawer.SnapSlots[0] != null)
            {
                planePosition = activeDrawer.SnapSlots[0].position;
            }

            Plane plane = new Plane(planeNormal, planePosition);
            if (plane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                draggedItem.transform.position = hitPoint;

                Quaternion baseFlatRot = draggedItem.GetFlatBaseRotation(anchor);
                draggedItem.transform.rotation = Quaternion.AngleAxis(currentDragYRotation, anchor.up) * baseFlatRot;
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

    private void StartDragFromStorage(Transform slot, InteractableItem item, DragSource source)
    {
        currentDragSource = source;
        originSlotTransform = slot;
        currentDragYRotation = item.transform.localEulerAngles.y;

        if (source == DragSource.Drawer && activeDrawer != null) activeDrawer.RemoveItem(item);
        else if (source == DragSource.Cabinet && activeCabinet != null) activeCabinet.RemoveItem(item);

        draggedItem = item;
        if (draggedItem.TryGetComponent(out Collider col)) col.enabled = false;
        if (draggedItem.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
    }

    private void EndDrag()
    {
        // 1. Check for a nearby shelf slot using a precise 1.2f search threshold
        Transform targetSlot = null;
        float maxSnapDistance = 1.2f;

        if (activeDrawer != null)
        {
            targetSlot = activeDrawer.GetClosestFreeSlot(draggedItem.transform.position, maxSnapDistance);
        }
        else if (activeCabinet != null)
        {
            targetSlot = activeCabinet.GetClosestFreeSlot(draggedItem.transform.position, maxSnapDistance);
        }

        // If directly over a shelf slot, snap to shelf
        if (targetSlot != null)
        {
            if (activeDrawer != null) activeDrawer.PlaceItemInSlot(draggedItem, targetSlot, currentDragYRotation);
            else if (activeCabinet != null) activeCabinet.PlaceItemInSlot(draggedItem, targetSlot, currentDragYRotation);

            ClearDragState();
            UpdateInventoryVisuals(true);
            return;
        }

        // 2. If NOT aligned with a shelf slot, check if dropped over Hotbar UI
        if (IsPointerOverInventoryUI())
        {
            if (inventory.Items.Count < 5)
            {
                draggedItem.Collect(inventory);
                ClearDragState();
                UpdateInventoryVisuals(true);
                return;
            }
        }

        // 3. Fallback: Return to starting location if released in open space
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
        else if (currentDragSource == DragSource.Drawer && originSlotTransform != null && activeDrawer != null)
        {
            activeDrawer.PlaceItemInSlot(draggedItem, originSlotTransform, currentDragYRotation);
        }
        else if (currentDragSource == DragSource.Cabinet && originSlotTransform != null && activeCabinet != null)
        {
            activeCabinet.PlaceItemInSlot(draggedItem, originSlotTransform, currentDragYRotation);
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