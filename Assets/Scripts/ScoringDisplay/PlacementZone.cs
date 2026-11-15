using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlacementZone : MonoBehaviour
{
    [Header("Zone Validation Rules")]
    [SerializeField] private bool allowSmallItems = true;
    [SerializeField] private bool allowLargeItems = true;
    [SerializeField] private bool requireMatchingTag = true;
    [SerializeField] private ItemTag requiredTag = ItemTag.Needed;

    [Header("Surface Snap Alignment")]
    [SerializeField] private bool snapToSurfaceHeight = true;
    [SerializeField] private float surfaceYOffset = 0.005f;

    [Header("Optional Scoring Reference")]
    [SerializeField] private ScoreDisplay scoreDisplay;
    [SerializeField] private bool awardScoreOnPlace = true;

    [Header("Diagnostics")]
    [SerializeField] private bool enableDebugLogs = true;

    private readonly List<InteractableItem> placedItems = new List<InteractableItem>();

    void Awake()
    {
        if (scoreDisplay == null)
        {
            scoreDisplay = FindFirstObjectByType<ScoreDisplay>(FindObjectsInactive.Include);
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else if (enableDebugLogs)
        {
            Debug.LogError($"[PlacementZone:{name}] Missing Collider component!");
        }
    }

    void Update()
    {
        for (int i = placedItems.Count - 1; i >= 0; i--)
        {
            InteractableItem item = placedItems[i];
            if (item == null || !item.gameObject.activeInHierarchy || item.transform.parent != transform)
            {
                ReleaseItemAtIndex(i);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TryCaptureItem(other, logRejection: enableDebugLogs);
    }

    private void OnTriggerStay(Collider other)
    {
        TryCaptureItem(other, logRejection: false);
    }

    private void OnTriggerExit(Collider other)
    {
        InteractableItem item = other.GetComponentInParent<InteractableItem>();
        if (item != null && placedItems.Contains(item))
        {
            ReleaseItem(item);
        }
    }

    /// <summary>
    /// Cleanly removes an item from this PlacementZone, deducts points, and unparents it.
    /// </summary>
    public void ReleaseItem(InteractableItem item)
    {
        if (item == null) return;

        int index = placedItems.IndexOf(item);
        if (index != -1)
        {
            ReleaseItemAtIndex(index);
        }
        else if (item.transform.parent == transform)
        {
            item.transform.SetParent(null);
        }
    }

    private void ReleaseItemAtIndex(int index)
    {
        InteractableItem item = placedItems[index];
        placedItems.RemoveAt(index);

        if (item != null)
        {
            if (item.transform.parent == transform)
            {
                item.transform.SetParent(null);
            }

            if (enableDebugLogs)
            {
                Debug.Log($"[PlacementZone:{name}] Item '{item.ItemName}' removed from zone.");
            }

            ApplyScore(item, isPlacing: false);

            if (item.gameObject.activeInHierarchy && item.transform.parent == null && !item.IsInStorage)
            {
                if (item.TryGetComponent(out Rigidbody rb))
                {
                    rb.isKinematic = false;
                    rb.useGravity = true;
                }
            }
        }

        UpdateGlobalObjectiveProgress();
    }

    private void TryCaptureItem(Collider other, bool logRejection)
    {
        InteractableItem item = other.GetComponentInParent<InteractableItem>();
        if (item == null) return;
        if (placedItems.Contains(item)) return;

        if (item.IsInStorage)
        {
            if (logRejection)
                Debug.Log($"[PlacementZone:{name}] Rejected '{item.ItemName}' — item.IsInStorage is true.");
            return;
        }

        if (item.Size == ItemSize.Small && !allowSmallItems)
        {
            if (logRejection)
                Debug.Log($"[PlacementZone:{name}] Rejected '{item.ItemName}' — Small items are disabled on this zone.");
            return;
        }

        if (item.Size == ItemSize.Large && !allowLargeItems)
        {
            if (logRejection)
                Debug.Log($"[PlacementZone:{name}] Rejected '{item.ItemName}' — Large items are disabled on this zone.");
            return;
        }

        // Ignore items still actively held by the player
        if (IsItemHeldByPlayer(item))
        {
            if (logRejection)
                Debug.Log($"[PlacementZone:{name}] Waiting on '{item.ItemName}' — still held by player.");
            return;
        }

        // Ignore items already claimed by a different PlacementZone
        PlacementZone existingZone = item.GetComponentInParent<PlacementZone>();
        if (existingZone != null && existingZone != this) return;

        StickItemToSurface(item);
    }

    private bool IsItemHeldByPlayer(InteractableItem item)
    {
        if (item.TryGetComponent(out Rigidbody rb) && rb.isKinematic)
        {
            return true;
        }

        if (item.GetComponentInParent<Camera>() != null || item.GetComponentInParent<CharacterController>() != null)
        {
            return true;
        }

        return false;
    }

    private void StickItemToSurface(InteractableItem item)
    {
        placedItems.Add(item);

        MovableObject.NotifyItemPickedUp(item);

        if (item.TryGetComponent(out Rigidbody rb))
        {
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (snapToSurfaceHeight)
        {
            AlignItemToSurface(item);
        }

        item.transform.SetParent(transform);

        Collider[] colliders = item.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = true;
        }

        ApplyScore(item, isPlacing: true);
        UpdateGlobalObjectiveProgress();
    }

    private void AlignItemToSurface(InteractableItem item)
    {
        Collider itemCol = item.GetComponentInChildren<Collider>();
        if (itemCol == null) return;

        float pivotToBottom = item.transform.position.y - itemCol.bounds.min.y;
        Vector3 rayOrigin = new Vector3(item.transform.position.x, itemCol.bounds.max.y + 0.3f, item.transform.position.z);

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 1.5f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (!hit.collider.transform.IsChildOf(item.transform))
            {
                Vector3 snappedPos = item.transform.position;
                snappedPos.y = hit.point.y + pivotToBottom + surfaceYOffset;
                item.transform.position = snappedPos;
            }
        }
    }

    public bool IsItemInCorrectZone(InteractableItem item)
    {
        if (item == null || !placedItems.Contains(item)) return false;
        if (item.Size == ItemSize.Small && !allowSmallItems) return false;
        if (item.Size == ItemSize.Large && !allowLargeItems) return false;
        if (requireMatchingTag && item.Tag != requiredTag) return false;

        return true;
    }

    private void ApplyScore(InteractableItem item, bool isPlacing)
    {
        if (!awardScoreOnPlace || item == null) return;

        if (scoreDisplay == null)
        {
            scoreDisplay = FindFirstObjectByType<ScoreDisplay>(FindObjectsInactive.Include);
            if (scoreDisplay == null) return;
        }

        int delta = 0;

        if (item.Tag == ItemTag.Needed)
        {
            delta = isPlacing ? item.TablePlacementPoints : -item.TablePlacementPoints;
        }
        else if (item.Tag == ItemTag.Trash)
        {
            delta = isPlacing ? -item.TrashPoints : item.TrashPoints;
        }

        if (delta != 0)
        {
            scoreDisplay.AddScore(delta);
            if (enableDebugLogs)
                Debug.Log($"[PlacementZone:{name}] {(delta > 0 ? "Added" : "Removed")} {Mathf.Abs(delta)} score for '{item.ItemName}' (Tag: {item.Tag}).");
        }
    }

    private void UpdateGlobalObjectiveProgress()
    {
        if (LevelObjectiveManager.Instance == null) return;

        int totalCorrectlyPlaced = 0;
        PlacementZone[] allZones = FindObjectsByType<PlacementZone>(FindObjectsSortMode.None);
        InteractableItem[] allItems = FindObjectsByType<InteractableItem>(FindObjectsSortMode.None);

        foreach (var item in allItems)
        {
            if (item == null || !item.gameObject.activeInHierarchy || item.Tag != ItemTag.Needed) continue;

            if (item.IsInStorage)
            {
                totalCorrectlyPlaced++;
                continue;
            }

            foreach (var zone in allZones)
            {
                if (zone != null && zone.IsItemInCorrectZone(item))
                {
                    totalCorrectlyPlaced++;
                    break;
                }
            }
        }

        LevelObjectiveManager.Instance.ReportPlacementChange(totalCorrectlyPlaced);
    }
}