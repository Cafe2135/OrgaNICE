using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4.5f;
    [SerializeField] private float crouchSpeed = 2.25f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Look")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 85f;

    [Header("Crouch")]
    [SerializeField] private float standingHeight = 1.8f;
    [SerializeField] private float crouchingHeight = 1.0f;
    [SerializeField] private float standingCameraY = 1.6f;
    [SerializeField] private float crouchingCameraY = 0.9f;
    [SerializeField] private float crouchTransitionSpeed = 8f;

    private CharacterController controller;
    private Vector3 velocity;
    private float verticalLookRotation;
    private bool isCrouching;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        // Lock the cursor for mouse-look. Once a pause/settings menu exists,
        // move this to whatever manages UI state so it can unlock on pause.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleLook();
        HandleCrouch();
        HandleMovement();
    }

    private void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Yaw rotates the whole body; pitch only rotates the camera pivot.
        transform.Rotate(Vector3.up * mouseX);

        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -maxLookAngle, maxLookAngle);
        cameraPivot.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);
    }

    private void HandleCrouch()
    {
        isCrouching = Input.GetKey(KeyCode.LeftShift);

        float targetHeight = isCrouching ? crouchingHeight : standingHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);

        Vector3 center = controller.center;
        center.y = controller.height / 2f;
        controller.center = center;

        float targetCameraY = isCrouching ? crouchingCameraY : standingCameraY;
        Vector3 camPos = cameraPivot.localPosition;
        camPos.y = Mathf.Lerp(camPos.y, targetCameraY, crouchTransitionSpeed * Time.deltaTime);
        cameraPivot.localPosition = camPos;
    }

    private void HandleMovement()
    {
        float speed = isCrouching ? crouchSpeed : walkSpeed;

        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");
        Vector3 move = (transform.right * inputX + transform.forward * inputZ).normalized * speed;

        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f; // small constant downward force keeps the controller grounded on slopes
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move((move + Vector3.up * velocity.y) * Time.deltaTime);
    }
}