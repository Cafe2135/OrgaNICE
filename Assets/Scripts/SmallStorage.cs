using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallStorage : MonoBehaviour
{
    [Header("Cabinet & Sliding Motion")]
    [SerializeField] private Transform slidingTransform; // The drawer mesh/transform that slides
    [SerializeField] private Vector3 slideDirection = Vector3.forward; // Local direction to pull out (e.g. forward or z-axis)
    [SerializeField] private float slideDistance = 0.55f; // How far out the drawer pulls
    [SerializeField] private float slideSpeed = 6f; // Speed of opening/closing animation

    [Header("Camera & Slots")]
    [SerializeField] private Transform cameraAnchor;
    [SerializeField] private List<Transform> snapSlots = new List<Transform>();

    [Header("References")]
    [SerializeField] private PlacementZone placementZone;

    private readonly Dictionary<Transform, InteractableItem> slotOccupants = new Dictionary<Transform, InteractableItem>();

    private Vector3 closedLocalPos;
    private Vector3 openLocalPos;
    private Coroutine slideRoutine;

    public Transform CameraAnchor => cameraAnchor;
    public List<Transform> SnapSlots => snapSlots;
    public bool IsOpen { get; private set; } = false;

    void Awake()
    {
        if (slidingTransform == null) slidingTransform = transform;

        closedLocalPos = slidingTransform.localPosition;
        openLocalPos = closedLocalPos + (slideDirection.normalized * slideDistance);

        foreach (var slot in snapSlots)
        {
            if (slot != null && !slotOccupants.ContainsKey(slot))
            {
                slotOccupants[slot] = null;
            }
        }

        if (placementZone == null) placementZone = GetComponent<PlacementZone>();
    }

    public void OpenDrawer()
    {
        IsOpen = true;
        AnimateSlide(openLocalPos);
    }

    public void CloseDrawer()
    {
        IsOpen = false;
        AnimateSlide(closedLocalPos);
    }

    private void AnimateSlide(Vector3 targetLocalPos)
    {
        if (slideRoutine != null) StopCoroutine(slideRoutine);
        slideRoutine = StartCoroutine(SlideRoutine(targetLocalPos));
    }

    private IEnumerator SlideRoutine(Vector3 targetLocalPos)
    {
        while (Vector3.Distance(slidingTransform.localPosition, targetLocalPos) > 0.001f)
        {
            slidingTransform.localPosition = Vector3.Lerp(slidingTransform.localPosition, targetLocalPos, Time.deltaTime * slideSpeed);
            yield return null;
        }
        slidingTransform.localPosition = targetLocalPos;
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
        item.transform.SetParent(targetSlot); // Parent item to slot so it moves along when drawer closes!
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
        if (item != null)
        {
            item.transform.SetParent(null); // Unparent when leaving drawer
            item.UnlockFromStorage();
        }
    }
}