using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class VehicleHeadlightController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Danh sách đèn xe (Spot/Point Light)")]
    public List<Light> headLights = new List<Light>();

    [Tooltip("Text hint UI (TMP). Nếu đang dùng InteractHint UI thì kéo Text vào đây.")]
    public TextMeshProUGUI hintText;

    [Header("Input")]
    public KeyCode toggleKey = KeyCode.F;

    [Header("Light Settings")]
    [Tooltip("Cường độ khi bật. (URP có thể cần lớn hơn 10-50 tuỳ scene)")]
    public float onIntensity = 30f;

    [Header("Behavior")]
    [Tooltip("Nếu tick, khi xuống xe sẽ tự tắt đèn.")]
    public bool turnOffWhenDismount = false;

    [Header("Debug")]
    public bool debugLogs = true;

    // ===== INTERNAL STATE =====
    [SerializeField] private bool isMounted = false;
    [SerializeField] private bool isLightOn = false;

    void Start()
    {
        if (debugLogs)
        {
            Debug.Log($"[VehicleHeadlight] Start() | lights={headLights.Count} | hintText={(hintText ? hintText.name : "NULL")}");
        }

        // Mặc định: tắt đèn khi vào game
        ApplyLights(false, "Start");
        UpdateHint("Start");
    }

    void Update()
    {
        // Không ngồi xe -> ẩn hint, không nhận phím
        if (!isMounted)
        {
            ClearHint();
            return;
        }

        // Có ngồi xe -> luôn hiện hint
        UpdateHint("Update");

        if (Input.GetKeyDown(toggleKey))
        {
            if (debugLogs) Debug.Log("[VehicleHeadlight] Update() detected F -> ToggleLights()");
            ToggleLights();
        }
    }

    // =====================================================
    // Public API - VehicleInteractable sẽ gọi 2 hàm này
    // =====================================================
    public void OnMounted()
    {
        isMounted = true;

        if (debugLogs)
            Debug.Log("[VehicleHeadlight] OnMounted() -> isMounted = TRUE");

        // Khi vừa lên xe: hiện hint ngay
        UpdateHint("OnMounted");
    }

    public void OnDismounted()
    {
        isMounted = false;

        if (debugLogs)
            Debug.Log("[VehicleHeadlight] OnDismounted() -> isMounted = FALSE");

        if (turnOffWhenDismount)
        {
            if (debugLogs) Debug.Log("[VehicleHeadlight] turnOffWhenDismount enabled -> Force lights OFF");
            isLightOn = false;
            ApplyLights(false, "OnDismounted");
        }

        ClearHint();
    }

    // =====================================================
    // Core
    // =====================================================
    void ToggleLights()
    {
        isLightOn = !isLightOn;

        if (debugLogs)
            Debug.Log($"[VehicleHeadlight] ToggleLights() -> {(isLightOn ? "ON" : "OFF")}");

        ApplyLights(isLightOn, "ToggleLights");
        UpdateHint("ToggleLights");
    }

    void ApplyLights(bool on, string caller)
    {
        if (headLights == null || headLights.Count == 0)
        {
            if (debugLogs)
                Debug.LogWarning($"[VehicleHeadlight] ApplyLights({on}) called by {caller} BUT headLights list is empty");
            return;
        }

        int applied = 0;

        foreach (var light in headLights)
        {
            if (!light) continue;

            light.enabled = on;

            // Chỉ set intensity khi bật
            if (on) light.intensity = onIntensity;

            applied++;
        }

        if (debugLogs)
            Debug.Log($"[VehicleHeadlight] ApplyLights({on}) by {caller} | applied={applied} | intensity={(on ? onIntensity : 0f)}");
    }

    // =====================================================
    // Hint
    // =====================================================
    void UpdateHint(string caller)
    {
        if (!hintText)
        {
            if (debugLogs)
                Debug.LogWarning($"[VehicleHeadlight] UpdateHint() by {caller} but hintText is NULL");
            return;
        }

        if (!isMounted)
        {
            hintText.text = "";
            return;
        }

        hintText.text = isLightOn ? "Ấn [F] để tắt đèn" : "Ấn [F] để bật đèn";

        if (debugLogs)
            Debug.Log($"[VehicleHeadlight] UpdateHint() by {caller} -> \"{hintText.text}\"");
    }

    void ClearHint()
    {
        if (!hintText) return;

        if (!string.IsNullOrEmpty(hintText.text))
        {
            if (debugLogs) Debug.Log("[VehicleHeadlight] ClearHint()");
        }

        hintText.text = "";
    }
}
