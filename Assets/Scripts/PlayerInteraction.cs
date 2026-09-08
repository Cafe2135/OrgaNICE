using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask interactableLayer;

    [Header("Grab")]
    [SerializeField] private float grabRange = 3f;
    [SerializeField] private float followSpeed = 12f;

    [Header("Hold Distance")]
    [SerializeField] private float minHoldDistance = 0.6f;
    [SerializeField] private float maxHoldDistance = 3f;
    [SerializeField] private float scrollSpeed = 2f;

    [Header("Rotate")]
    [SerializeField] private float rotateSpeed = 90f; // degrees per second

    private Rigidbody heldBody;
    private float currentHoldDistance;
    private int selectedAxis = 1; // 0 = X, 1 = Y, 2 = Z
    private static readonly Vector3[] Axes = { Vector3.right, Vector3.up, Vector3.forward };

    public bool IsHolding => heldBody != null;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) TryGrab();
        else if (Input.GetMouseButtonUp(0)) Release();

        HandleAxisSelection();

        if (heldBody != null)
        {
            HandleScrollDistance();
        }
    }

    void FixedUpdate()
    {
        if (heldBody != null)
        {
            MoveHeldObject();
            RotateHeldObject();
        }
    }

    private void TryGrab()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, grabRange, interactableLayer))
        {
            if (hit.rigidbody != null)
            {
                heldBody = hit.rigidbody;
                heldBody.useGravity = false;
                currentHoldDistance = Mathf.Clamp(
                    Vector3.Distance(playerCamera.transform.position, heldBody.position),
                    minHoldDistance, maxHoldDistance);
            }
        }
    }

    private void Release()
    {
        if (heldBody == null) return;

        heldBody.useGravity = true;
        heldBody.linearVelocity = Vector3.zero;
        heldBody = null;
    }

    private void MoveHeldObject()
    {
        Vector3 targetPoint = playerCamera.transform.position + playerCamera.transform.forward * currentHoldDistance;
        Vector3 toTarget = targetPoint - heldBody.position;
        heldBody.linearVelocity = toTarget * followSpeed;
    }

    private void RotateHeldObject()
    {
        float direction = 0f;
        if (Input.GetKey(KeyCode.Q)) direction = -1f;
        else if (Input.GetKey(KeyCode.E)) direction = 1f;

        if (direction != 0f)
        {
            Quaternion delta = Quaternion.AngleAxis(direction * rotateSpeed * Time.fixedDeltaTime, Axes[selectedAxis]);
            heldBody.MoveRotation(delta * heldBody.rotation);
        }
    }

    private void HandleScrollDistance()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            currentHoldDistance = Mathf.Clamp(currentHoldDistance + scroll * scrollSpeed, minHoldDistance, maxHoldDistance);
        }
    }

    private void HandleAxisSelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) selectedAxis = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2)) selectedAxis = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3)) selectedAxis = 2;
        else if (Input.GetKeyDown(KeyCode.C)) selectedAxis = (selectedAxis + 1) % 3;
    }
}