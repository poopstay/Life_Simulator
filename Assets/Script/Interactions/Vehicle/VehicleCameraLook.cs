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
    public float minYaw = -75f;
    public float maxYaw = 75f;

    [Header("Roll (lean when turning)")]
    public float maxRollAngle = 8f;
    public float rollSmooth = 6f;

    [Header("Debug")]
    public bool debugLogs = true;

    private Vector2 velocity;       // (x = yawAccum, y = pitchAccum inverted usage)
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

        yaw = 0f;
        pitch = 0f;
        currentRoll = 0f;

        if (debugLogs) Debug.Log("[VehicleCameraLook] SetActive(" + on + ") | reset velocity/yaw/pitch/roll");
    }

    private void Awake()
    {
        if (debugLogs) Debug.Log("[VehicleCameraLook] Awake()");
    }

    private void Update()
    {
        if (!activeLook) return;

        // ===== 1) Read mouse delta =====
        Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        Vector2 raw = mouseDelta * sensitivity;

        // smoothing (giống kiểu FirstPersonLook)
        float lerpT = 1f / Mathf.Max(0.01f, smoothing);
        frameVelocity = Vector2.Lerp(frameVelocity, raw, lerpT);

        // ===== 2) Accumulate =====
        velocity += frameVelocity;

        // ===== 3) Clamp "THẬT" (không để tích lũy vượt) =====
        // yaw lấy trực tiếp từ velocity.x
        float unclampedYaw = velocity.x;
        yaw = Mathf.Clamp(unclampedYaw, minYaw, maxYaw);

        // pitch: bạn đang dùng -velocity.y nên giữ nguyên logic
        float unclampedPitch = -velocity.y;
        pitch = Mathf.Clamp(unclampedPitch, minPitch, maxPitch);

        // IMPORTANT:
        // Sau khi clamp, ghi ngược lại vào velocity để không còn "dư"
        // velocity.x phải = yaw
        // velocity.y phải = -pitch (vì pitch = -velocity.y)
        velocity.x = yaw;
        velocity.y = -pitch;

        // ===== 4) Roll when A/D =====
        float targetRoll = 0f;
        if (Input.GetKey(KeyCode.A)) targetRoll = maxRollAngle;
        else if (Input.GetKey(KeyCode.D)) targetRoll = -maxRollAngle;

        currentRoll = Mathf.Lerp(currentRoll, targetRoll, Time.deltaTime * rollSmooth);

        // ===== 5) Apply rotations =====
        // Rig yaw
        transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

        // Child camera pitch + roll
        if (transform.childCount > 0)
        {
            Transform cam = transform.GetChild(0);
            cam.localRotation = Quaternion.Euler(pitch, 0f, currentRoll);
        }
        else
        {
            if (debugLogs) Debug.LogWarning("[VehicleCameraLook] No child camera found under rig!");
        }

        // ===== 6) Debug =====
        if (debugLogs)
        {
            // Log khi bạn kéo mạnh (để thấy clamp hoạt động)
            if (Mathf.Abs(mouseDelta.x) > 0.01f || Mathf.Abs(mouseDelta.y) > 0.01f)
            {
                Debug.Log(
                    $"[VehicleCameraLook] " +
                    $"raw=({raw.x:0.00},{raw.y:0.00}) " +
                    $"frameVel=({frameVelocity.x:0.00},{frameVelocity.y:0.00}) " +
                    $"yaw={yaw:0.0}/{minYaw:0.0}..{maxYaw:0.0} " +
                    $"pitch={pitch:0.0}/{minPitch:0.0}..{maxPitch:0.0} " +
                    $"roll={currentRoll:0.0}"
                );
            }
        }
    }
}
