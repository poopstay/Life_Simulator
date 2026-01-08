using UnityEngine;

public class NightFogSystem : MonoBehaviour
{
    [Header("References")]
    public GameTimeSystem timeSystem;

    [Header("Fog Settings")]
    public Color nightFogColor = new Color(0.08f, 0.1f, 0.15f);
    public float dayFogDensity = 0.0005f;
    public float nightFogDensity = 0.01f;

    [Header("Time Settings")]
    public float fogStartHour = 18f;
    public float fogEndHour = 6f;
    public float fadeDurationHours = 1.0f;

    [Header("Debug")]
    public bool debugLogs = true;

    void Start()
    {
        if (!timeSystem)
        {
            Debug.LogError("[NightFog] GameTimeSystem NOT assigned");
            enabled = false;
            return;
        }

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;

        Debug.Log("[NightFog] Initialized");
    }

    void Update()
    {
        UpdateFog();
    }

    void UpdateFog()
    {
        float hour = timeSystem.CurrentHourFloat;

        float density = CalculateFogDensity(hour);
        RenderSettings.fogDensity = density;

        if (density > dayFogDensity)
            RenderSettings.fogColor = nightFogColor;

        if (debugLogs)
        {
            Debug.Log($"[NightFog] Hour={hour:0.00} | FogDensity={density:0.000}");
        }
    }

    float CalculateFogDensity(float hour)
    {
        // 🌙 Đêm hoàn toàn
        if (hour >= fogStartHour + fadeDurationHours || hour < fogEndHour)
            return nightFogDensity;

        // 🌅 Fade sương tối
        if (hour >= fogStartHour && hour < fogStartHour + fadeDurationHours)
        {
            float t = Mathf.InverseLerp(
                fogStartHour,
                fogStartHour + fadeDurationHours,
                hour
            );
            return Mathf.Lerp(dayFogDensity, nightFogDensity, t);
        }

        // 🌄 Tan sương sáng
        if (hour >= fogEndHour - fadeDurationHours && hour < fogEndHour)
        {
            float t = Mathf.InverseLerp(
                fogEndHour - fadeDurationHours,
                fogEndHour,
                hour
            );
            return Mathf.Lerp(nightFogDensity, dayFogDensity, t);
        }

        return dayFogDensity;
    }
}
