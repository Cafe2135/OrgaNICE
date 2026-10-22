using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovableObject : MonoBehaviour
{
    [SerializeField] private string objectName = "Ironing Board";
    [SerializeField] private float dragFollowSpeed = 12f;

    private Rigidbody rb;

    public string ObjectName => objectName;
    public float DragFollowSpeed => dragFollowSpeed;
    public Rigidbody Rb => rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.useGravity = true;
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
                    Physics.IgnoreCollision(myCol, itemCol, true);
                }
            }
        }
    }
}