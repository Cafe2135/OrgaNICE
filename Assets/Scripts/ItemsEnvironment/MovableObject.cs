using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovableObject : MonoBehaviour
{
    [SerializeField] private string objectName = "Ironing Board";
    [SerializeField] private float dragFollowSpeed = 12f;
    [SerializeField] private bool enableDebugLogs = true;

    private Rigidbody rb;

    public string ObjectName => objectName;
    public float DragFollowSpeed => dragFollowSpeed;
    public Rigidbody Rb => rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Unity ignores non-convex MeshColliders on moving Rigidbodies.
        // Force Convex = true on solid colliders so picked-up items can physically rest on top.
        MeshCollider[] meshColliders = GetComponentsInChildren<MeshCollider>();
        foreach (var meshCol in meshColliders)
        {
            if (meshCol != null && !meshCol.isTrigger && !meshCol.convex)
            {
                meshCol.convex = true;
            }
        }
    }

    void Start()
    {
        IgnoreAllItemCollisionsAtStart();
    }

    /// <summary>
    /// At game start, ignores collisions with all items so items hidden underneath/behind
    /// this movable object do not cause physics explosions or get pushed prematurely.
    /// </summary>
    public void IgnoreAllItemCollisionsAtStart()
    {
        InteractableItem[] items = FindObjectsByType<InteractableItem>(FindObjectsSortMode.None);
        int count = 0;

        foreach (var item in items)
        {
            if (item != null)
            {
                SetCollisionIgnoredWithItem(item, true);
                count++;
            }
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[MovableObject:{objectName}] Ignored start collisions with {count} items in scene.");
        }
    }

    /// <summary>
    /// Enables or disables physics collision between this MovableObject and a specific InteractableItem.
    /// </summary>
    public void SetCollisionIgnoredWithItem(InteractableItem item, bool ignore)
    {
        if (item == null) return;

        Collider[] myColliders = GetComponentsInChildren<Collider>();
        Collider[] itemColliders = item.GetComponentsInChildren<Collider>();

        foreach (var myCol in myColliders)
        {
            if (myCol == null || myCol.isTrigger) continue;

            foreach (var itemCol in itemColliders)
            {
                if (itemCol == null || itemCol.isTrigger) continue;
                Physics.IgnoreCollision(myCol, itemCol, ignore);
            }
        }

        if (enableDebugLogs && !ignore)
        {
            Debug.Log($"[MovableObject:{objectName}] Re-enabled solid collisions with '{item.ItemName}'.");
        }
    }

    /// <summary>
    /// Call this once when the player picks up/discovers an item.
    /// Restores solid collisions between that item and ALL MovableObjects in the scene.
    /// </summary>
    public static void NotifyItemPickedUp(InteractableItem item)
    {
        if (item == null) return;

        MovableObject[] allMovables = FindObjectsByType<MovableObject>(FindObjectsSortMode.None);
        foreach (var movable in allMovables)
        {
            if (movable != null)
            {
                movable.SetCollisionIgnoredWithItem(item, false);
            }
        }
    }
}