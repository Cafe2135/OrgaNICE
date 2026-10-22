using UnityEngine;

public class HeavyObjectController : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private float interactRange = 3.0f;
    [SerializeField] private LayerMask movableLayer = ~0;

    private MovableObject currentMovable;
    private Vector3 localOffsetFromPlayer;
    private bool isDragging = false;

    public static bool IsDraggingObject { get; private set; } = false;

    void Awake()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (playerInteraction == null) playerInteraction = GetComponent<PlayerInteraction>();
        if (playerInteraction == null) playerInteraction = FindFirstObjectByType<PlayerInteraction>();
    }

    void Update()
    {
        // Block dragging heavy objects if paused, evaluating, in storage view, OR holding/inspecting an item
        if (PauseMenu.IsPaused || 
            LevelEvaluationUI.IsEvaluating || 
            StorageInteractionController.IsInStorageMode || 
            (playerInteraction != null && playerInteraction.IsHolding))
        {
            if (isDragging) StopDragging();
            return;
        }

        // Hold Left-Click to grab and drag
        if (Input.GetMouseButtonDown(0) && !isDragging)
        {
            TryStartDragging();
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            StopDragging();
        }
    }

    void FixedUpdate()
    {
        if (isDragging && currentMovable != null)
        {
            MoveObjectWithPlayer();
        }
    }

    private void TryStartDragging()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, movableLayer, QueryTriggerInteraction.Collide))
        {
            MovableObject movable = hit.collider.GetComponentInParent<MovableObject>();
            if (movable != null)
            {
                currentMovable = movable;
                isDragging = true;
                IsDraggingObject = true;

                // Calculate relative offset from player position in local space
                localOffsetFromPlayer = transform.InverseTransformPoint(currentMovable.transform.position);

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    private void MoveObjectWithPlayer()
    {
        Vector3 targetWorldPos = transform.TransformPoint(localOffsetFromPlayer);
        
        Vector3 displacement = targetWorldPos - currentMovable.transform.position;
        Vector3 moveVelocity = new Vector3(displacement.x, 0f, displacement.z) * currentMovable.DragFollowSpeed;

        currentMovable.Rb.linearVelocity = new Vector3(moveVelocity.x, currentMovable.Rb.linearVelocity.y, moveVelocity.z);
    }

    private void StopDragging()
    {
        if (currentMovable != null && currentMovable.Rb != null)
        {
            currentMovable.Rb.linearVelocity = new Vector3(0f, currentMovable.Rb.linearVelocity.y, 0f);
        }

        currentMovable = null;
        isDragging = false;
        IsDraggingObject = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}