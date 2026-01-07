using UnityEngine;

public class VehicleCameraLook : MonoBehaviour
{
    [Header("Sensitivity")]
    public float sensitivity = 2.0f;
    public float smoothing = 1.5f;

    [Header("Clamp Pitch (up/down)")]
    public float minPitch = -25f;
    public float maxPitch = 25f;

    [Header("Clamp Yaw (left/right)")]
    public float minYaw = -75f;   // giới hạn nhìn trái
    public float maxYaw = 75f;    // giới hạn nhìn phải

    [Header("Roll (lean when turning)")]
    public float maxRollAngle = 8f;
    public float rollSmooth = 6f;

    [Header("Debug")]
    public bool debugLogs = true;

    private Vector2 velocity;
    private Vector2 frameVelocity;

    private float yaw;
    private float pitch;
    private float currentRoll;

    private bool activeLook;

    public void SetActive(bool on)
    {
        activeLook = on;
        frameVelocity = Vector2.zero;
        velocity = Vector2.zero;

        if (debugLogs) Debug.Log("[VehicleCameraLook] SetActive(" + on + ")");
    }

    private void Awake()
    {
        if (debugLogs) Debug.Log("[VehicleCameraLook] Awake()");
    }

    private void Update()
    {
        if (!activeLook) return;

        // Mouse delta
        Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        Vector2 raw = mouseDelta * sensitivity;
        frameVelocity = Vector2.Lerp(frameVelocity, raw, 1f / Mathf.Max(0.01f, smoothing));

        // accumulate
        velocity += frameVelocity;

        // ===== Clamp yaw/pitch =====
        yaw = Mathf.Clamp(velocity.x, minYaw, maxYaw);
        pitch = Mathf.Clamp(-velocity.y, minPitch, maxPitch);

        // ===== Roll when A/D =====
        float targetRoll = 0f;
        if (Input.GetKey(KeyCode.A)) targetRoll = maxRollAngle;
        else if (Input.GetKey(KeyCode.D)) targetRoll = -maxRollAngle;

        currentRoll = Mathf.Lerp(currentRoll, targetRoll, Time.deltaTime * rollSmooth);

        // ===== Apply =====
        // Rig: yaw
        transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

        // Child camera: pitch + roll
        if (transform.childCount > 0)
        {
            Transform cam = transform.GetChild(0);
            cam.localRotation = Quaternion.Euler(pitch, 0f, currentRoll);
        }

        if (debugLogs && Input.GetMouseButtonDown(0))
            Debug.Log($"[VehicleCameraLook] yaw={yaw:0.0} pitch={pitch:0.0} roll={currentRoll:0.0}");
    }
}
