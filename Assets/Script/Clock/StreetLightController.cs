using UnityEngine;

public class StreetLightController : MonoBehaviour
{
    [Header("References")]
    public GameTimeSystem timeSystem;

    [Tooltip("Toàn bộ đèn đường")]
    public Light[] streetLights;

    [Header("Time Settings")]
    [Tooltip("Giờ bắt đầu bật đèn")]
    public float turnOnHour = 18f;

    [Tooltip("Giờ tắt hẳn đèn")]
    public float turnOffHour = 6f;

    [Tooltip("Thời gian fade (giờ)")]
    public float fadeDurationHours = 1.0f; // 18h → 19h fade sáng

    [Header("Light Settings")]
    public float maxIntensity = 3.0f;

    [Header("Debug")]
    public bool debugLogs = true;

    void Start()
    {
        if (!timeSystem)
        {
            Debug.LogError("[StreetLight] GameTimeSystem NOT assigned");
            enabled = false;
            return;
        }

        if (streetLights == null || streetLights.Length == 0)
        {
            Debug.LogError("[StreetLight] No street lights assigned");
            enabled = false;
            return;
        }

        Debug.Log("[StreetLight] Initialized");
    }

    void Update()
    {
        UpdateLights();
    }

    void UpdateLights()
    {
        float hour = timeSystem.CurrentHourFloat;
        float intensity = CalculateIntensity(hour);

        foreach (Light l in streetLights)
        {
            if (!l) continue;

            l.enabled = intensity > 0.01f;
            l.intensity = intensity;
        }

        if (debugLogs)
        {
            Debug.Log($"[StreetLight] Hour={hour:0.00} | Intensity={intensity:0.00}");
        }
    }

    float CalculateIntensity(float hour)
    {
        // 🌙 Ban đêm hoàn toàn
        if (hour >= turnOnHour + fadeDurationHours || hour < turnOffHour)
            return maxIntensity;

        // 🌅 Fade sáng
        if (hour >= turnOnHour && hour < turnOnHour + fadeDurationHours)
        {
            float t = Mathf.InverseLerp(
                turnOnHour,
                turnOnHour + fadeDurationHours,
                hour
            );
            return Mathf.Lerp(0f, maxIntensity, t);
        }

        // 🌄 Fade tắt (sáng sớm)
        if (hour >= turnOffHour - fadeDurationHours && hour < turnOffHour)
        {
            float t = Mathf.InverseLerp(
                turnOffHour - fadeDurationHours,
                turnOffHour,
                hour
            );
            return Mathf.Lerp(maxIntensity, 0f, t);
        }

        // ☀ Ban ngày
        return 0f;
    }
}
