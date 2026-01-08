using UnityEngine;

public class DayNightSystem : MonoBehaviour
{
    [Header("References")]
    public Light sunLight;              // MUST be Directional Light
    public GameTimeSystem timeSystem;

    [Header("Time Keys")]
    [Tooltip("Giờ mặt trời bắt đầu sáng rõ (bình minh)")]
    public float sunriseHour = 6f;

    [Tooltip("Giờ trưa")]
    public float noonHour = 12f;

    [Tooltip("Giờ bắt đầu tối (hoàng hôn)")]
    public float sunsetHour = 18f;

    [Tooltip("Giờ tối hẳn")]
    public float nightHour = 19f;

    [Header("Sun Rotation (X angle)")]
    [Tooltip("Góc X của sun lúc bình minh")]
    public float sunriseAngle = 10f;

    [Tooltip("Góc X của sun lúc trưa (cao nhất)")]
    public float noonAngle = 70f;

    [Tooltip("Góc X của sun lúc hoàng hôn")]
    public float sunsetAngle = 10f;

    [Tooltip("Góc Y cố định để hướng nắng")]
    public float sunYaw = 30f;

    [Header("Sun Intensity (URP thường cần cao hơn 1)")]
    public float nightIntensity = 0.0f;
    public float dayIntensity = 2.5f;   // tăng lên cho chắc sáng 07:30
    public float duskIntensity = 0.6f;  // ánh sáng chiều tối

    [Header("Sun Color")]
    public Color nightColor = new Color(0.12f, 0.12f, 0.2f);
    public Color dayColor = Color.white;
    public Color duskColor = new Color(1.0f, 0.55f, 0.35f);

    [Header("Ambient (optional but recommended)")]
    public bool controlAmbient = true;
    public Color ambientNight = new Color(0.06f, 0.06f, 0.10f);
    public Color ambientDay = new Color(0.35f, 0.35f, 0.35f);
    public float ambientIntensityDay = 1.2f;
    public float ambientIntensityNight = 0.4f;

    [Header("Debug")]
    public bool debugLogs = true;
    public bool logEveryFrame = false;

    void Start()
    {
        if (!sunLight)
        {
            Debug.LogError("[DayNightSystem] sunLight NOT assigned");
            enabled = false;
            return;
        }

        if (sunLight.type != LightType.Directional)
        {
            Debug.LogError("[DayNightSystem] sunLight is NOT Directional. Bạn đang kéo nhầm đèn (Spot/Point) vào!");
            enabled = false;
            return;
        }

        if (!timeSystem)
        {
            Debug.LogError("[DayNightSystem] GameTimeSystem NOT assigned");
            enabled = false;
            return;
        }

        if (debugLogs)
        {
            Debug.Log("[DayNightSystem] Initialized OK");
            Debug.Log($"[DayNightSystem] dayIntensity={dayIntensity} | nightIntensity={nightIntensity} | sunriseHour={sunriseHour} | sunsetHour={sunsetHour}");
        }
    }

    void Update()
    {
        UpdateSunAndAmbient();
    }

    void UpdateSunAndAmbient()
    {
        float hour = timeSystem.CurrentHourFloat;

        // ===== 1) ROTATION (X angle) =====
        float sunAngleX = CalcSunAngleX(hour);
        sunLight.transform.rotation = Quaternion.Euler(sunAngleX, sunYaw, 0f);

        // ===== 2) INTENSITY =====
        float intensity = CalcSunIntensity(hour);
        sunLight.intensity = intensity;

        // ===== 3) COLOR =====
        Color c = CalcSunColor(hour);
        sunLight.color = c;

        // ===== 4) AMBIENT (optional) =====
        if (controlAmbient)
        {
            float tDay = DayFactor(hour); // 0..1
            RenderSettings.ambientLight = Color.Lerp(ambientNight, ambientDay, tDay);
            RenderSettings.ambientIntensity = Mathf.Lerp(ambientIntensityNight, ambientIntensityDay, tDay);
        }

        if (debugLogs && (logEveryFrame || Time.frameCount % 60 == 0))
        {
            Debug.Log($"[DayNightSystem] Hour={hour:0.00} | SunX={sunAngleX:0.0} | Int={intensity:0.00} | DayFactor={DayFactor(hour):0.00}");
        }
    }

    float CalcSunAngleX(float hour)
    {
        // Đêm: giữ thấp
        if (hour < sunriseHour || hour >= sunsetHour)
            return -10f;

        // Sáng: sunrise -> noon
        if (hour <= noonHour)
        {
            float t = Mathf.InverseLerp(sunriseHour, noonHour, hour);
            return Mathf.Lerp(sunriseAngle, noonAngle, t);
        }

        // Chiều: noon -> sunset
        {
            float t = Mathf.InverseLerp(noonHour, sunsetHour, hour);
            return Mathf.Lerp(noonAngle, sunsetAngle, t);
        }
    }

    float CalcSunIntensity(float hour)
    {
        // Tối hẳn: night -> sunrise
        if (hour < sunriseHour || hour >= nightHour)
            return nightIntensity;

        // Sunrise -> day
        if (hour >= sunriseHour && hour < sunriseHour + 1f)
        {
            float t = Mathf.InverseLerp(sunriseHour, sunriseHour + 1f, hour);
            return Mathf.Lerp(nightIntensity, dayIntensity, t);
        }

        // Day (7h -> 17h) giữ sáng mạnh
        if (hour >= sunriseHour + 1f && hour < sunsetHour - 1f)
            return dayIntensity;

        // Dusk fade (sunset-1 -> nightHour)
        {
            float t = Mathf.InverseLerp(sunsetHour - 1f, nightHour, hour);
            return Mathf.Lerp(dayIntensity, duskIntensity, t);
        }
    }

    Color CalcSunColor(float hour)
    {
        // Đêm
        if (hour < sunriseHour || hour >= nightHour)
            return nightColor;

        // Bình minh -> ban ngày
        if (hour < sunriseHour + 1.5f)
        {
            float t = Mathf.InverseLerp(sunriseHour, sunriseHour + 1.5f, hour);
            return Color.Lerp(duskColor, dayColor, t);
        }

        // Chiều -> hoàng hôn
        if (hour > sunsetHour - 1.5f)
        {
            float t = Mathf.InverseLerp(sunsetHour - 1.5f, sunsetHour, hour);
            return Color.Lerp(dayColor, duskColor, t);
        }

        return dayColor;
    }

    // 0..1: 0 = đêm, 1 = ngày
    float DayFactor(float hour)
    {
        // day factor dựa vào khoảng [sunriseHour .. nightHour]
        if (hour < sunriseHour || hour >= nightHour) return 0f;
        if (hour >= sunriseHour + 1f && hour <= sunsetHour - 1f) return 1f;

        // ramp up/down
        if (hour < sunriseHour + 1f)
            return Mathf.InverseLerp(sunriseHour, sunriseHour + 1f, hour);

        return Mathf.InverseLerp(nightHour, sunsetHour - 1f, hour); // đảo (nightHour -> sunsetHour-1)
    }
}
