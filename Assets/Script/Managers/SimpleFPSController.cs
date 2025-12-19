using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class SimpleFPSController_NewInput : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1.2f;
    public float runSpeed = 2.2f;
    public float gravity = -9.81f;
    public float jumpHeight = 0.3f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 0.5f;
    public Transform cameraTransform;

    [Header("Stamina")]
    public float maxStamina = 5f;
    public float staminaDrain = 1.5f;
    public float staminaRecover = 1f;

    [Header("Camera FOV")]
    public float normalFOV = 60f;
    public float runFOV = 75f;
    public float fovSmooth = 8f;

    [Header("Head Bob")]
    public float walkBobSpeed = 6f;
    public float walkBobAmount = 0.02f;
    public float runBobSpeed = 10f;
    public float runBobAmount = 0.045f;
    public float bobSmooth = 12f;

    private CharacterController controller;
    private Camera cam;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool jumpPressed;
    private bool isRunning;

    private float verticalVelocity;
    private float verticalLookRotation;
    private float currentStamina;

    private float bobTimer;
    private Vector3 cameraStartLocalPos;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        cam = cameraTransform.GetComponent<Camera>();
        currentStamina = maxStamina;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cam.fieldOfView = normalFOV;
        cameraStartLocalPos = cameraTransform.localPosition;
    }

    private void Update()
    {
        ReadInput();
        HandleMouseLook();
        HandleMovement();
        HandleStamina();
        HandleFOV();
        HandleHeadBob();
    }

    // ================= INPUT =================
    private void ReadInput()
    {
        // MOVE
        moveInput = Keyboard.current != null
            ? new Vector2(
                (Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0),
                (Keyboard.current.wKey.isPressed ? 1 : 0) - (Keyboard.current.sKey.isPressed ? 1 : 0)
              )
            : Vector2.zero;

        // LOOK
        lookInput = Mouse.current != null
            ? Mouse.current.delta.ReadValue()
            : Vector2.zero;

        // JUMP
        jumpPressed = Keyboard.current != null &&
                      Keyboard.current.spaceKey.wasPressedThisFrame;

        // RUN
        bool wantsRun =
            Keyboard.current != null &&
            Keyboard.current.leftShiftKey.isPressed &&
            moveInput.y > 0f &&
            currentStamina > 0f;

        if (wantsRun && controller.isGrounded)
            isRunning = true;

        if (!wantsRun)
            isRunning = false;
    }

    // ================= LOOK =================
    private void HandleMouseLook()
    {
        float mouseX = lookInput.x * mouseSensitivity * 0.01f;
        float mouseY = lookInput.y * mouseSensitivity * 0.01f;

        transform.Rotate(Vector3.up * mouseX);

        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -80f, 80f);
        cameraTransform.localEulerAngles = new Vector3(verticalLookRotation, 0f, 0f);
    }

    // ================= MOVE =================
    private void HandleMovement()
    {
        Vector3 moveDir = (transform.forward * moveInput.y + transform.right * moveInput.x).normalized;

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        if (jumpPressed && controller.isGrounded)
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        verticalVelocity += gravity * Time.deltaTime;

        float speed = isRunning ? runSpeed : moveSpeed;

        Vector3 velocity = moveDir * speed + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }

    // ================= STAMINA =================
    private void HandleStamina()
    {
        if (isRunning)
            currentStamina -= staminaDrain * Time.deltaTime;
        else if (controller.isGrounded)
            currentStamina += staminaRecover * Time.deltaTime;

        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

        if (currentStamina <= 0f)
            isRunning = false;
    }

    // ================= FOV =================
    private void HandleFOV()
    {
        float targetFOV = isRunning ? runFOV : normalFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * fovSmooth);
    }

    // ================= HEAD BOB =================
    private void HandleHeadBob()
    {
        if (!controller.isGrounded || moveInput.magnitude < 0.1f)
        {
            cameraTransform.localPosition = Vector3.Lerp(
                cameraTransform.localPosition,
                cameraStartLocalPos,
                Time.deltaTime * bobSmooth
            );
            return;
        }

        float bobSpeed = isRunning ? runBobSpeed : walkBobSpeed;
        float bobAmount = isRunning ? runBobAmount : walkBobAmount;

        bobTimer += Time.deltaTime * bobSpeed;

        float offsetY = Mathf.Sin(bobTimer) * bobAmount;
        float offsetX = Mathf.Cos(bobTimer * 0.5f) * bobAmount * 0.5f;

        Vector3 targetPos = cameraStartLocalPos + new Vector3(offsetX, offsetY, 0f);

        cameraTransform.localPosition = Vector3.Lerp(
            cameraTransform.localPosition,
            targetPos,
            Time.deltaTime * bobSmooth
        );
    }
}
