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
        
        // Lock X and Z rotation so heavy objects remain strictly upright and never tip over
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.useGravity = true;
    }
}