using System.Collections.Generic;
using UnityEngine;

public class SmallStorage : MonoBehaviour
{
    [Header("Camera & Slots")]
    [SerializeField] private Transform cameraAnchor;
    [SerializeField] private List<Transform> snapSlots = new List<Transform>();

    [Header("References")]
    [SerializeField] private PlacementZone placementZone;

    private readonly Dictionary<Transform, InteractableItem> slotOccupants = new Dictionary<Transform, InteractableItem>();

    public Transform CameraAnchor => cameraAnchor;
    public List<Transform> SnapSlots => snapSlots;

    void Awake()
    {
        foreach (var slot in snapSlots)
        {
            if (slot != null && !slotOccupants.ContainsKey(slot))
            {
                slotOccupants[slot] = null;
            }
        }

        if (placementZone == null) placementZone = GetComponent<PlacementZone>();
    }

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
        item.transform.position = targetSlot.position;

        Quaternion baseFlatRot = item.GetFlatBaseRotation(targetSlot);
        item.transform.rotation = Quaternion.AngleAxis(currentYRotation, targetSlot.up) * baseFlatRot;

        if (item.TryGetComponent(out Collider col)) col.enabled = true;

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
        if (item != null) item.UnlockFromStorage();
    }
}