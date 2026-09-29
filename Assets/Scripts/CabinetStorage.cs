using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CabinetStorage : MonoBehaviour
{
    public enum RotationAxis { YAxis, XAxis, ZAxis }

    [Header("Door References")]
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;

    [Header("Rotation Settings")]
    [SerializeField] private RotationAxis rotationAxis = RotationAxis.YAxis;
    [SerializeField] private float openAngleLeft = -100f;
    [SerializeField] private float openAngleRight = 100f;
    [SerializeField] private float doorSpeed = 5f;

    [Header("Camera & Shelf Slots")]
    [SerializeField] private Transform cameraAnchor;
    [SerializeField] private List<Transform> snapSlots = new List<Transform>();

    [Header("Scoring System Reference")]
    [SerializeField] private ScoreDisplay scoreDisplay;

    private readonly Dictionary<Transform, InteractableItem> slotOccupants = new Dictionary<Transform, InteractableItem>();

    private Quaternion leftClosedRot;
    private Quaternion leftOpenRot;
    private Quaternion rightClosedRot;
    private Quaternion rightOpenRot;

    private Coroutine doorRoutine;

    public Transform CameraAnchor => cameraAnchor;
    public List<Transform> SnapSlots => snapSlots;
    public bool IsOpen { get; private set; } = false;

    void Awake()
    {
        if (scoreDisplay == null) scoreDisplay = FindFirstObjectByType<ScoreDisplay>();

        // Save initial closed rotations
        if (leftDoor != null) leftClosedRot = leftDoor.localRotation;
        if (rightDoor != null) rightClosedRot = rightDoor.localRotation;

        RecalculateTargetRotations();

        foreach (var slot in snapSlots)
        {
            if (slot != null && !slotOccupants.ContainsKey(slot))
            {
                slotOccupants[slot] = null;
            }
        }
    }

    void Update()
    {
        if (IsOpen && doorRoutine == null)
        {
            RecalculateTargetRotations();

            if (leftDoor != null) leftDoor.localRotation = leftOpenRot;
            if (rightDoor != null) rightDoor.localRotation = rightOpenRot;
        }
    }

    private void RecalculateTargetRotations()
    {
        if (leftDoor != null) leftOpenRot = leftClosedRot * GetAxisRotation(openAngleLeft);
        if (rightDoor != null) rightOpenRot = rightClosedRot * GetAxisRotation(openAngleRight);
    }

    private Quaternion GetAxisRotation(float angle)
    {
        switch (rotationAxis)
        {
            case RotationAxis.XAxis: return Quaternion.Euler(angle, 0f, 0f);
            case RotationAxis.ZAxis: return Quaternion.Euler(0f, 0f, angle);
            case RotationAxis.YAxis:
            default: return Quaternion.Euler(0f, angle, 0f);
        }
    }

    public void OpenCabinet()
    {
        if (IsOpen) return;
        IsOpen = true;
        RecalculateTargetRotations();
        AnimateDoors(leftOpenRot, rightOpenRot);
    }

    public void CloseCabinet()
    {
        if (!IsOpen) return;
        IsOpen = false;
        AnimateDoors(leftClosedRot, rightClosedRot);
    }

    private void AnimateDoors(Quaternion targetLeft, Quaternion targetRight)
    {
        if (doorRoutine != null) StopCoroutine(doorRoutine);
        doorRoutine = StartCoroutine(SlerpDoorRoutine(targetLeft, targetRight));
    }

    private IEnumerator SlerpDoorRoutine(Quaternion targetLeft, Quaternion targetRight)
    {
        Quaternion startLeft = leftDoor != null ? leftDoor.localRotation : Quaternion.identity;
        Quaternion startRight = rightDoor != null ? rightDoor.localRotation : Quaternion.identity;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * doorSpeed;

            if (leftDoor != null)
                leftDoor.localRotation = Quaternion.Slerp(startLeft, targetLeft, t);

            if (rightDoor != null)
                rightDoor.localRotation = Quaternion.Slerp(startRight, targetRight, t);

            yield return null;
        }

        if (leftDoor != null) leftDoor.localRotation = targetLeft;
        if (rightDoor != null) rightDoor.localRotation = targetRight;

        doorRoutine = null;
    }

    // --- Slot & Drag-and-Drop Management with Scoring ---

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

        if (item.TryGetComponent(out Collider col)) col.enabled = true;

        // --- SCORE & OBJECTIVE CALCULATION ---
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
            // --- SCORE & OBJECTIVE REVERSAL ---
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
                // Small needed items in cabinets give positive points
                scoreDelta = isPlacing ? points : -points;
                break;

            case ItemTag.Trash:
                // Trash items in cabinets deduct points
                scoreDelta = isPlacing ? -points : points;
                break;

            case ItemTag.Neutral:
                break;
        }

        if (scoreDelta != 0)
        {
            scoreDisplay.AddScore(scoreDelta);
        }
    }

    private void UpdateGlobalObjectiveProgress()
    {
        if (LevelObjectiveManager.Instance == null) return;

        int totalCorrectlyPlaced = 0;

        // 1. Count items in PlacementZones (Table surfaces & sliding drawers)
        PlacementZone[] allZones = FindObjectsByType<PlacementZone>(FindObjectsSortMode.None);
        foreach (var zone in allZones)
        {
            // Reflection check or direct zone iteration
            var zoneField = typeof(PlacementZone).GetField("itemsInZone", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (zoneField != null)
            {
                var items = zoneField.GetValue(zone) as List<InteractableItem>;
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        if (item != null && item.gameObject.activeInHierarchy && item.Tag == ItemTag.Needed && zone.IsItemInCorrectZone(item))
                        {
                            totalCorrectlyPlaced++;
                        }
                    }
                }
            }
        }

        // 2. Count small needed items stored in CabinetStorage slots
        CabinetStorage[] allCabinets = FindObjectsByType<CabinetStorage>(FindObjectsSortMode.None);
        foreach (var cabinet in allCabinets)
        {
            foreach (var pair in cabinet.slotOccupants)
            {
                InteractableItem item = pair.Value;
                if (item != null && item.gameObject.activeInHierarchy && item.Tag == ItemTag.Needed && item.Size == ItemSize.Small && item.IsInStorage)
                {
                    totalCorrectlyPlaced++;
                }
            }
        }

        LevelObjectiveManager.Instance.ReportPlacementChange(totalCorrectlyPlaced);
    }
}