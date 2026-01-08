using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleInteractable : MonoBehaviour, IInteractable
{
    [Header("Hint Text")]
    public string mountHint = "Ấn [E] để lên xe";
    public string dismountHint = "Ấn [E] để xuống xe";
    public string brakeHint = "Giữ [Space] để phanh";
    public string boostHint = "Giữ [Shift] để tăng tốc";
    public string lightHint = "Ấn [F] để tắt/mở đèn";	
    public string mustStopHint = "Xe phải dừng hẳn mới xuống được";
	public VehicleHeadlightController headlight;
    [Header("Seat / Mount point")]
    public Transform seatPoint;

    [Header("Disable on mount (Player components)")]
    [Tooltip("Kéo movement/jump/rotate/look... KHÔNG kéo PlayerInput. Interactor có thể để enabled.")]
    public MonoBehaviour[] disableOnMount;

    [Header("Camera Switch")]
    public Camera vehicleCamera;       // VehicleCam
    public Camera playerCamera;        // First Person Camera
    public bool disablePlayerCameraOnMount = true;

    [Header("Vehicle")]
    public VehicleMotor motor;
    public VehicleCameraLook vehicleLook;

    [Header("Player Visual")]
    [Tooltip("Root model của player (vd: Ch31_nonPBR). Để trống vẫn tự tìm.")]
    public GameObject playerVisualRoot;

    [Header("Dismount")]
    public float dismountSideOffset = 0.8f;

    [Header("Mounted Hint Mode")]
    [Tooltip("Khi đã lên xe: luôn show hint (không phụ thuộc raycast).")]
    public bool alwaysShowHintWhenMounted = true;

    [Tooltip("Ngăn Mount xong bị bắt E và xuống ngay lập tức.")]
    public float blockDismountSecondsAfterMount = 0.25f;

    [Header("Debug")]
    public bool debugLogs = true;

    // ===== runtime state =====
    private bool isMounted;
    private float mountedAtTime;

    private Transform mountedPlayer;
    private Interactor mountedInteractor;

    private Camera cachedInteractorCam;

    private void Awake()
    {
        if (debugLogs) Debug.Log($"[VehicleInteractable] Awake() on '{name}'");

        if (!motor) motor = GetComponentInParent<VehicleMotor>();
        if (!vehicleLook) vehicleLook = GetComponentInChildren<VehicleCameraLook>(true);
    }

    private void Update()
    {
        // NOTE: không show/hide hint ở đây vì Interactor.Update cũng chạy -> sẽ “giật” UI
        if (!isMounted) return;

        // Bắt phím bằng NEW Input System để chắc chắn hoạt động
        // (trong project bạn khả năng đang để Input System Package Only)
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.eKey.wasPressedThisFrame)
        {
            if (Time.time - mountedAtTime < blockDismountSecondsAfterMount)
            {
                if (debugLogs) Debug.Log("[VehicleInteractable] Update() E ignored (just mounted) -> return");
                return;
            }

            if (debugLogs) Debug.Log("[VehicleInteractable] Update() detected E -> TryDismount()");
            TryDismount();
        }
    }

    private void LateUpdate()
    {
        // LateUpdate chạy sau Interactor.Update -> ta “đè” hint lên để Interactor không hide được
        if (!isMounted) return;
        if (!alwaysShowHintWhenMounted) return;

        ForceMountedHintUI();
    }

    // =========================
    // IInteractable
    // =========================
    public string GetHintText()
    {
        // Hint cho trạng thái CHƯA mount (raycast)
        if (!isMounted) return mountHint;

        // nếu vẫn raycast trúng (không quan trọng)
        if (motor != null && !motor.IsStopped) return $"{brakeHint} - {boostHint} - {lightHint}";
        return dismountHint;
    }

    public void OnFocus() { }
    public void OnUnfocus() { }

    public void Interact(Interactor interactor)
    {
        if (debugLogs) Debug.Log("[VehicleInteractable] Interact() called");

        if (!isMounted) Mount(interactor);
        else TryDismount();
    }

    // =========================
    // Mount / Dismount
    // =========================
    private void Mount(Interactor interactor)
    {
		if (headlight) headlight.OnMounted();
		
        if (debugLogs) Debug.Log("[VehicleInteractable] Mount() begin");

        if (isMounted)
        {
            if (debugLogs) Debug.Log("[VehicleInteractable] Already mounted -> return");
            return;
        }

        if (!interactor)
        {
            Debug.LogWarning("[VehicleInteractable] interactor NULL -> return");
            return;
        }

        if (!seatPoint)
        {
            Debug.LogWarning("[VehicleInteractable] seatPoint NULL -> return");
            return;
        }

        if (!motor)
        {
            Debug.LogWarning("[VehicleInteractable] motor NULL -> return (hãy add VehicleMotor vào Vehicle root)");
            return;
        }

        mountedInteractor = interactor;
        mountedPlayer = interactor.transform;

        cachedInteractorCam = mountedInteractor.cam;

        if (!playerVisualRoot)
        {
            // tìm root model player
            var smr = mountedPlayer.GetComponentInChildren<SkinnedMeshRenderer>(true);
            if (smr) playerVisualRoot = smr.transform.root.gameObject;
        }

        if (debugLogs)
            Debug.Log($"[VehicleInteractable] Mount refs OK | seatPoint='{seatPoint.name}' motor='{motor.name}' player='{mountedPlayer.name}'");

        // Disable player scripts
        SetDisableOnMount(true);

        // Hide player model
        if (playerVisualRoot)
        {
            playerVisualRoot.SetActive(false);
            if (debugLogs) Debug.Log("[VehicleInteractable] Hide playerVisualRoot: " + playerVisualRoot.name);
        }

        // Snap to seat
        mountedPlayer.position = seatPoint.position;
        mountedPlayer.rotation = Quaternion.Euler(0f, seatPoint.eulerAngles.y, 0f);

        // Switch camera
        SwitchToVehicleCamera(true);

        // Đổi cam raycast của Interactor sang VehicleCam (để tương tác khác nếu cần)
        if (mountedInteractor && vehicleCamera)
        {
            mountedInteractor.cam = vehicleCamera;
            if (debugLogs) Debug.Log("[VehicleInteractable] Interactor.cam -> VehicleCam (" + vehicleCamera.name + ")");
        }

        // Enable vehicle controls
        motor.SetMounted(true);
        if (vehicleLook) vehicleLook.SetActive(true);

        isMounted = true;
        mountedAtTime = Time.time;

        if (debugLogs) Debug.Log("[VehicleInteractable] Mounted SUCCESS");

        // Show hint ngay lập tức (lần 1)
        ForceMountedHintUI();
    }

    private void TryDismount()
    {
		if (headlight) headlight.OnDismounted();
		
        if (debugLogs) Debug.Log("[VehicleInteractable] TryDismount()");

        if (!isMounted)
        {
            if (debugLogs) Debug.Log("[VehicleInteractable] Not mounted -> return");
            return;
        }

        // Must stop
        if (motor && !motor.IsStopped)
        {
            if (debugLogs) Debug.Log("[VehicleInteractable] Block dismount: motor not stopped -> " + mustStopHint);

            // vẫn show hint phanh/tăng tốc
            ForceMountedHintUI();
            return;
        }

        // Disable vehicle controls
        if (vehicleLook) vehicleLook.SetActive(false);
        if (motor) motor.SetMounted(false);

        // Restore camera
        SwitchToVehicleCamera(false);

        // Restore Interactor cam
        if (mountedInteractor)
        {
            mountedInteractor.cam = cachedInteractorCam ? cachedInteractorCam : playerCamera;
            if (debugLogs) Debug.Log("[VehicleInteractable] Interactor.cam restored -> " + (mountedInteractor.cam ? mountedInteractor.cam.name : "NULL"));
        }

        // Move player aside
        if (mountedPlayer && seatPoint)
        {
            mountedPlayer.position = seatPoint.position + seatPoint.right * Mathf.Max(0f, dismountSideOffset);
        }

        // Show player model
        if (playerVisualRoot) playerVisualRoot.SetActive(true);

        // Enable player scripts
        SetDisableOnMount(false);

        // Hide hint UI (Interactor sẽ tự show lại theo raycast)
        if (mountedInteractor && mountedInteractor.hintUI) mountedInteractor.hintUI.Hide();

        mountedPlayer = null;
        mountedInteractor = null;
        cachedInteractorCam = null;
        isMounted = false;

        if (debugLogs) Debug.Log("[VehicleInteractable] Dismounted SUCCESS");
    }

    // =========================
    // ALWAYS-ON HINT (override Interactor hide)
    // =========================
    private void ForceMountedHintUI()
    {
        if (!mountedInteractor)
        {
            if (debugLogs) Debug.Log("[VehicleInteractable] ForceMountedHintUI() mountedInteractor NULL -> return");
            return;
        }

        if (!mountedInteractor.hintUI)
        {
            if (debugLogs) Debug.Log("[VehicleInteractable] ForceMountedHintUI() hintUI NULL -> return");
            return;
        }

        if (!motor)
        {
            if (debugLogs) Debug.Log("[VehicleInteractable] ForceMountedHintUI() motor NULL -> return");
            return;
        }

        string msg = (!motor.IsStopped)
            ? $"{brakeHint} - {boostHint} - {lightHint}"
            : dismountHint;

        mountedInteractor.hintUI.Show(msg);

        // Optional: đổi màu crosshair trạng thái "đang điều khiển"
        if (mountedInteractor.crosshair) mountedInteractor.crosshair.SetInteractable(true);

        if (debugLogs && Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
            Debug.Log("[VehicleInteractable] ForceMountedHintUI -> " + msg.Replace("\n", " | "));
    }

    // =========================
    // Helpers
    // =========================
    private void SetDisableOnMount(bool disable)
    {
        if (disableOnMount == null) return;

        foreach (var mb in disableOnMount)
        {
            if (!mb) continue;

            // KHÔNG disable PlayerInput
            if (mb.GetType().Name == "PlayerInput")
            {
                if (debugLogs) Debug.Log("[VehicleInteractable] Skip disabling PlayerInput");
                continue;
            }

            mb.enabled = !disable;

            if (debugLogs)
                Debug.Log($"[VehicleInteractable] {(disable ? "Disabled" : "Enabled")} {mb.GetType().Name} on {mb.gameObject.name}");
        }
    }

    private void SwitchToVehicleCamera(bool toVehicle)
    {
        if (!vehicleCamera)
        {
            Debug.LogWarning("[VehicleInteractable] vehicleCamera NULL -> cannot switch");
            return;
        }

        if (!playerCamera)
        {
            var main = Camera.main;
            if (main) playerCamera = main;
        }

        if (debugLogs)
            Debug.Log($"[VehicleInteractable] SwitchToVehicleCamera({toVehicle}) | playerCam={(playerCamera ? playerCamera.name : "NULL")} vehicleCam={vehicleCamera.name}");

        if (toVehicle)
        {
            if (playerCamera && disablePlayerCameraOnMount) playerCamera.gameObject.SetActive(false);
            vehicleCamera.gameObject.SetActive(true);

            vehicleCamera.tag = "MainCamera";
            if (playerCamera) playerCamera.tag = "Untagged";
        }
        else
        {
            if (playerCamera) playerCamera.gameObject.SetActive(true);
            vehicleCamera.gameObject.SetActive(false);

            if (playerCamera) playerCamera.tag = "MainCamera";
            vehicleCamera.tag = "Untagged";
        }
    }
}
