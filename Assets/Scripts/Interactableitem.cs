using UnityEngine;

public enum ItemTag
{
    Trash,
    Neutral,
    Needed
}

public enum ItemSize
{
    Small,
    Large
}

public enum UpAxis
{
    PositiveY,
    PositiveX,
    PositiveZ
}

public class InteractableItem : MonoBehaviour
{
    [SerializeField] private string itemName = "Item";
    [SerializeField] private Sprite icon;
    [SerializeField] private ItemTag itemTag = ItemTag.Neutral;
    [SerializeField] private ItemSize itemSize = ItemSize.Small;
    [SerializeField, TextArea] private string description = "An item.";

    [Header("Scoring")]
    [SerializeField] private int tablePlacementPoints = 100;
    [SerializeField] private int drawerPlacementPoints = 300;
    [SerializeField] private int trashPoints = 250;

    [Header("Drawer Flat Snapping")]
    [SerializeField] private UpAxis flatUpAxis = UpAxis.PositiveY;
    [SerializeField] private Vector3 customRotationOffset = Vector3.zero;

    public string ItemName => itemName;
    public Sprite Icon => icon;
    public ItemTag Tag => itemTag;
    public ItemSize Size => itemSize;
    public int TablePlacementPoints => tablePlacementPoints;
    public int DrawerPlacementPoints => drawerPlacementPoints;
    public int TrashPoints => trashPoints;

    public bool IsInStorage { get; private set; } = false;

    public Quaternion GetFlatBaseRotation(Transform slotTransform)
    {
        Vector3 localUpVector = Vector3.up;
        switch (flatUpAxis)
        {
            case UpAxis.PositiveX: localUpVector = Vector3.right; break;
            case UpAxis.PositiveZ: localUpVector = Vector3.forward; break;
            case UpAxis.PositiveY: localUpVector = Vector3.up; break;
        }

        Quaternion alignment = Quaternion.FromToRotation(localUpVector, Vector3.up);
        return slotTransform.rotation * alignment * Quaternion.Euler(customRotationOffset);
    }

    // Freeze physics only while snapped inside a drawer slot
    public void LockInStorage()
    {
        IsInStorage = true;
        if (TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    // Restore full 3D physics whenever item leaves a drawer or enters world
    public void UnlockFromStorage()
    {
        IsInStorage = false;
        if (TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.WakeUp();
        }
    }

    public void Collect(Inventory inventory)
    {
        if (IsInStorage) return;

        // Ensure item is standard dynamic physics before storing in inventory
        UnlockFromStorage();

        if (inventory.AddItem(itemName, description, icon, itemTag, gameObject))
        {
            gameObject.SetActive(false);
        }
    }
}