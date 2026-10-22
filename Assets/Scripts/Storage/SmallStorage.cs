using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallStorage : MonoBehaviour
{
    [Header("Drawer Movement Settings")]
    [Tooltip("The moving drawer mesh transform.")]
    [SerializeField] private Transform drawerTransform;

    [Tooltip("Vector direction for sliding (e.g., X: 0, Y: 0, Z: 1 for forward or Z: -1 for backward).")]
    [SerializeField] private Vector3 slideDirection = new Vector3(0f, 0f, 1f);

    [Tooltip("How far out the drawer slides when opened.")]
    [SerializeField] private float slideDistance = 0.4f;

    [Tooltip("Speed of the sliding animation.")]
    [SerializeField] private float slideSpeed = 5f;

    [Header("Camera & Shelf Slots")]
    [SerializeField] private Transform cameraAnchor;
    [SerializeField] private List<Transform> snapSlots = new List<Transform>();

    [Header("Scoring System Reference")]
    [SerializeField] private ScoreDisplay scoreDisplay;

    private readonly Dictionary<Transform, InteractableItem> slotOccupants = new Dictionary<Transform, InteractableItem>();

    private Vector3 closedLocalPos;
    private Coroutine slideRoutine;

    public Transform CameraAnchor => cameraAnchor;
    public List<Transform> SnapSlots => snapSlots;
    public bool IsOpen { get; private set; } = false;

    void Awake()
    {
        if (scoreDisplay == null) scoreDisplay = FindFirstObjectByType<ScoreDisplay>();

        if (drawerTransform != null)
        {
            closedLocalPos = drawerTransform.localPosition;
        }

        foreach (var slot in snapSlots)
        {
            if (slot != null && !slotOccupants.ContainsKey(slot))
            {
                slotOccupants[slot] = null;
            }
        }
    }

    public void OpenDrawer()
    {
        if (IsOpen) return;
        if (drawerTransform == null)
        {
            Debug.LogError($"[SmallStorage] Drawer Transform is not assigned on {gameObject.name}!");
            return;
        }

        IsOpen = true;

        // Calculate target open position dynamically using Direction * Distance
        Vector3 directionNormalized = slideDirection.normalized;
        if (slideDirection == Vector3.zero) directionNormalized = Vector3.forward;

        Vector3 targetOpenPos = closedLocalPos + (directionNormalized * slideDistance);
        AnimateDrawer(targetOpenPos);
    }

    public void CloseDrawer()
    {
        if (!IsOpen) return;
        if (drawerTransform == null) return;

        IsOpen = false;
        AnimateDrawer(closedLocalPos);
    }

    private void AnimateDrawer(Vector3 targetPos)
    {
        if (slideRoutine != null) StopCoroutine(slideRoutine);
        slideRoutine = StartCoroutine(SlideRoutine(targetPos));
    }

    private IEnumerator SlideRoutine(Vector3 targetPos)
    {
        while (Vector3.Distance(drawerTransform.localPosition, targetPos) > 0.001f)
        {
            drawerTransform.localPosition = Vector3.Lerp(drawerTransform.localPosition, targetPos, Time.deltaTime * slideSpeed);
            yield return null;
        }
        drawerTransform.localPosition = targetPos;
        slideRoutine = null;
    }

    // --- Slot & Drag-and-Drop Management ---

    public Transform GetClosestFreeSlot(Vector3 worldPoint, float maxDistance = 1.2f)
    {
        Transform bestSlot = null;
        float minDistance = maxDistance;

        foreach (var slot in snapSlots)
        {
            if (slot == null || slotOccupants[slot] != null) continue;

            float dist = Vector3.Distance(worldPoint, slot.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                bestSlot = slot;
            }
        }

        return bestSlot;
    }

    public bool PlaceItemInSlot(InteractableItem item, Transform targetSlot, float currentYRotation)
    {
        if (item == null || item.Size != ItemSize.Small) return false;
        if (!snapSlots.Contains(targetSlot) || slotOccupants[targetSlot] != null) return false;

        slotOccupants[targetSlot] = item;
        item.LockInStorage();

        item.gameObject.SetActive(true);
        item.transform.SetParent(targetSlot);
        item.transform.position = targetSlot.position;

        Quaternion baseFlatRot = item.GetFlatBaseRotation(targetSlot);
        item.transform.rotation = Quaternion.AngleAxis(currentYRotation, targetSlot.up) * baseFlatRot;

        Collider[] colliders = item.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = true;
        }

        ApplyScoreDelta(item, isPlacing: true);
        UpdateGlobalObjectiveProgress();

        return true;
    }

    public Transform GetSlotOfItem(InteractableItem item)
    {
        foreach (var pair in slotOccupants)
        {
            if (pair.Value == item) return pair.Key;
        }
        return null;
    }

    public void RemoveItem(InteractableItem item)
    {
        Transform slot = GetSlotOfItem(item);
        if (slot != null) slotOccupants[slot] = null;
        if (item != null)
        {
            ApplyScoreDelta(item, isPlacing: false);

            item.transform.SetParent(null);
            item.UnlockFromStorage();

            UpdateGlobalObjectiveProgress();
        }
    }

    private void ApplyScoreDelta(InteractableItem item, bool isPlacing)
    {
        if (scoreDisplay == null || item == null) return;

        int points = item.DrawerPlacementPoints;
        int scoreDelta = 0;

        switch (item.Tag)
        {
            case ItemTag.Needed:
                scoreDelta = isPlacing ? points : -points;
                break;

            case ItemTag.Trash:
                scoreDelta = isPlacing ? -points : points;
                break;
        }

        if (scoreDelta != 0) scoreDisplay.AddScore(scoreDelta);
    }

    private void UpdateGlobalObjectiveProgress()
    {
        if (LevelObjectiveManager.Instance == null) return;

        int totalCorrectlyPlaced = 0;

        InteractableItem[] allItems = FindObjectsByType<InteractableItem>(FindObjectsSortMode.None);
        foreach (var item in allItems)
        {
            if (item != null && 
                item.gameObject.activeInHierarchy && 
                item.Tag == ItemTag.Needed && 
                item.Size == ItemSize.Small && 
                item.IsInStorage)
            {
                totalCorrectlyPlaced++;
            }
        }

        LevelObjectiveManager.Instance.ReportPlacementChange(totalCorrectlyPlaced);
    }
}