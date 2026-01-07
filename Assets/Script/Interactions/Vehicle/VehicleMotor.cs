using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VehicleMotor : MonoBehaviour
{
    [Header("Speed (km/h)")]
    public float maxSpeedNormal = 20f;    // W thường
    public float maxSpeedBoost = 50f;     // giữ Shift
    public float accelKmhPerSec = 18f;    // tăng tốc mượt
    public float brakeKmhPerSec = 40f;    // phanh Space
    public float coastKmhPerSec = 8f;     // thả ga giảm dần

    [Header("Steer")]
    public float steerDegreesPerSec = 80f;
    public float steerWhileStopped = 30f;

    [Header("Stability (Anti-Tip)")]
    [Tooltip("Khóa nghiêng để xe không bị đổ: Freeze Rotation X & Z")]
    public bool freezeTiltRotation = true;

    [Tooltip("Hạ trọng tâm theo local Y (âm = thấp xuống). Ví dụ -0.35")]
    public float centerOfMassYOffset = -0.35f;

    [Tooltip("Drag khi đang lái")]
    public float dragMounted = 0.2f;

    [Tooltip("AngularDrag khi đang lái (tăng để bớt lắc)")]
    public float angularDragMounted = 30f;

    [Tooltip("Drag khi không lái (để xe nặng hơn, ít bị đẩy)")]
    public float dragUnmounted = 20f;

    [Tooltip("AngularDrag khi không lái")]
    public float angularDragUnmounted = 10f;

    [Header("Stop Detect")]
    public float stoppedKmhThreshold = 0.5f;

    [Header("Debug")]
    public bool debugLogs = true;

    private Rigidbody rb;
    private bool mounted;

    private float speedKmh; // tốc độ 1D tiến/lùi (km/h)
    private float lastLoggedSpeed;

    public float SpeedKmh => speedKmh;
    public bool IsStopped => Mathf.Abs(speedKmh) <= stoppedKmhThreshold;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (debugLogs) Debug.Log("[VehicleMotor] Awake()");

        ApplyStabilitySetup("Awake");
        ApplyDragForState("Awake");
    }

    private void ApplyStabilitySetup(string from)
    {
        if (!rb) return;

        // Hạ trọng tâm
        rb.centerOfMass = new Vector3(0f, centerOfMassYOffset, 0f);

        // Khóa nghiêng
        if (freezeTiltRotation)
        {
            rb.constraints |= RigidbodyConstraints.FreezeRotationX;
            rb.constraints |= RigidbodyConstraints.FreezeRotationZ;
        }

        if (debugLogs)
        {
            Debug.Log($"[VehicleMotor] ApplyStabilitySetup({from}) " +
                      $"| centerOfMassY={rb.centerOfMass.y:0.00} " +
                      $"| constraints={rb.constraints}");
        }
    }

    private void ApplyDragForState(string from)
    {
        if (!rb) return;

        if (mounted)
        {
            rb.linearDamping = dragMounted;
            rb.angularDamping = angularDragMounted;
        }
        else
        {
            rb.linearDamping = dragUnmounted;
            rb.angularDamping = angularDragUnmounted;
        }

        if (debugLogs)
            Debug.Log($"[VehicleMotor] ApplyDragForState({from}) mounted={mounted} drag={rb.linearDamping:0.00} angularDrag={rb.angularDamping:0.00}");
    }

    public void SetMounted(bool on)
    {
        mounted = on;

        if (debugLogs) Debug.Log("[VehicleMotor] SetMounted(" + on + ")");

        ApplyStabilitySetup("SetMounted");
        ApplyDragForState("SetMounted");

        // Nếu vừa xuống xe, có thể cho nó “dừng về 0” nhẹ nhàng
        // (tuỳ bạn muốn giữ quán tính hay không)
        // if (!mounted) speedKmh = Mathf.MoveTowards(speedKmh, 0f, 999f);
    }

    private void Update()
    {
        if (!mounted) return;

        bool w = Input.GetKey(KeyCode.W);
        bool s = Input.GetKey(KeyCode.S);
        bool a = Input.GetKey(KeyCode.A);
        bool d = Input.GetKey(KeyCode.D);

        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool brake = Input.GetKey(KeyCode.Space);

        float maxSpeed = shift ? maxSpeedBoost : maxSpeedNormal;

        if (brake)
        {
            float old = speedKmh;
            speedKmh = Mathf.MoveTowards(speedKmh, 0f, brakeKmhPerSec * Time.deltaTime);
            if (debugLogs && Mathf.Abs(old - speedKmh) > 0.2f)
                Debug.Log("[VehicleMotor] Brake -> speedKmh=" + speedKmh.ToString("0.0"));
        }
        else if (w)
        {
            float old = speedKmh;
            speedKmh = Mathf.MoveTowards(speedKmh, maxSpeed, accelKmhPerSec * Time.deltaTime);
            if (debugLogs && Mathf.Abs(old - speedKmh) > 0.2f)
                Debug.Log("[VehicleMotor] W accel -> speedKmh=" + speedKmh.ToString("0.0") + " / max=" + maxSpeed);
        }
        else if (s)
        {
            float reverseMax = -10f;
            float old = speedKmh;
            speedKmh = Mathf.MoveTowards(speedKmh, reverseMax, accelKmhPerSec * Time.deltaTime);
            if (debugLogs && Mathf.Abs(old - speedKmh) > 0.2f)
                Debug.Log("[VehicleMotor] S reverse -> speedKmh=" + speedKmh.ToString("0.0"));
        }
        else
        {
            float old = speedKmh;
            speedKmh = Mathf.MoveTowards(speedKmh, 0f, coastKmhPerSec * Time.deltaTime);
            if (debugLogs && Mathf.Abs(old - speedKmh) > 0.2f)
                Debug.Log("[VehicleMotor] Coast -> speedKmh=" + speedKmh.ToString("0.0"));
        }

        // Steering (yaw only)
        float steerInput = 0f;
        if (a) steerInput -= 1f;
        if (d) steerInput += 1f;

        if (Mathf.Abs(steerInput) > 0.01f)
        {
            float steerRate = IsStopped ? steerWhileStopped : steerDegreesPerSec;
            float yaw = steerInput * steerRate * Time.deltaTime;
            transform.Rotate(0f, yaw, 0f, Space.World);

            if (debugLogs) Debug.Log("[VehicleMotor] Steer " + (steerInput < 0 ? "Left" : "Right") + " yawDelta=" + yaw.ToString("0.00"));
        }

        if (debugLogs && Mathf.Abs(speedKmh - lastLoggedSpeed) > 3f)
        {
            lastLoggedSpeed = speedKmh;
            Debug.Log("[VehicleMotor] SpeedKmh=" + speedKmh.ToString("0.0") + " IsStopped=" + IsStopped);
        }
    }

    private void FixedUpdate()
    {
        if (!mounted) return;

        if (!rb)
        {
            Debug.LogWarning("[VehicleMotor] FixedUpdate rb NULL -> return");
            return;
        }

        // km/h -> m/s
        float speedMs = speedKmh / 3.6f;

        Vector3 forward = transform.forward;
        Vector3 targetVel = forward * speedMs;

        Vector3 vel = rb.linearVelocity;
        Vector3 newVel = new Vector3(targetVel.x, vel.y, targetVel.z);
        rb.linearVelocity = newVel;

        // Safety: nếu vẫn bị xoay “vô lý” thì dập bớt angular velocity
        // (thường do collider lệch, va chạm mạnh)
        if (freezeTiltRotation)
        {
            Vector3 av = rb.angularVelocity;
            rb.angularVelocity = new Vector3(0f, av.y, 0f);
        }

        if (debugLogs && IsStopped && rb.linearVelocity.magnitude > 1.0f)
            Debug.Log("[VehicleMotor] IsStopped but rb.velocity=" + rb.linearVelocity.magnitude.ToString("0.00"));
    }
}
