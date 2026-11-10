using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovableObject : MonoBehaviour
{
    [SerializeField] private string objectName = "Ironing Board";
    [SerializeField] private float dragFollowSpeed = 12f;

    [Header("Placement Surface Slots")]
    [Tooltip("Child Empty GameObjects positioned on top of the surface as snap points.")]
    [SerializeField] private List<Transform> surfaceSlots = new List<Transform>();

    private Rigidbody rb;
    private readonly Dictionary<Transform, InteractableItem> slotOccupants = new Dictionary<Transform, InteractableItem>();

    public string ObjectName => objectName;
    public float DragFollowSpeed => dragFollowSpeed;
    public Rigidbody Rb => rb;
    public List<Transform> SurfaceSlots => surfaceSlots;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.useGravity = true;

        foreach (var slot in surfaceSlots)
        {
            if (slot != null && !slotOccupants.ContainsKey(slot))
            {
                slotOccupants[slot] = null;
            }
        }
    }

    void Start()
    {
        IgnoreItemCollisions();
    }

    // Ignores collisions with all small/large interactable items in the scene
    public void IgnoreItemCollisions()
    {
        Collider[] myColliders = GetComponentsInChildren<Collider>();
        InteractableItem[] items = FindObjectsByType<InteractableItem>(FindObjectsSortMode.None);

        foreach (var item in items)
        {
            Collider[] itemColliders = item.GetComponentsInChildren<Collider>();
            foreach (var myCol in myColliders)
            {
                foreach (var itemCol in itemColliders)
                {
                    if (myCol != null && itemCol != null)
                    {
                        Physics.IgnoreCollision(myCol, itemCol, true);
                    }
                }
            }
        }
    }

    public Transform GetClosestFreeSlot(Vector3 worldPoint, float maxDistance = 1.0f)
    {
        Transform bestSlot = null;
        float minDistance = maxDistance;

        foreach (var slot in surfaceSlots)
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

    public bool PlaceItemOnSurface(InteractableItem item, Transform targetSlot)
    {
        if (item == null || targetSlot == null || !surfaceSlots.Contains(targetSlot) || slotOccupants[targetSlot] != null)
            return false;

        slotOccupants[targetSlot] = item;
        item.LockInStorage();

        item.gameObject.SetActive(true);
        item.transform.SetParent(targetSlot);
        item.transform.position = targetSlot.position;
        item.transform.rotation = targetSlot.rotation;

        // Freeze item physics so gravity doesn't pull it through the non-colliding table mesh
        if (item.TryGetComponent(out Rigidbody itemRb))
        {
            itemRb.isKinematic = true;
            itemRb.useGravity = false;
        }

        // Keep colliders enabled for mouse interaction and retrieval
        Collider[] colliders = item.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = true;
        }

        return true;
    }

    public void RemoveItemFromSurface(InteractableItem item)
    {
        Transform slot = GetSlotOfItem(item);
        if (slot != null) slotOccupants[slot] = null;

        if (item != null)
        {
            item.transform.SetParent(null);
            item.UnlockFromStorage();

            if (item.TryGetComponent(out Rigidbody itemRb))
            {
                itemRb.isKinematic = false;
                itemRb.useGravity = true;
            }
        }
    }

    public Transform GetSlotOfItem(InteractableItem item)
    {
        foreach (var pair in slotOccupants)
        {
            if (pair.Value == item) return pair.Key;
        }
        return null;
    }
}