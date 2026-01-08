using UnityEngine;
using TMPro;

public class GameTimeSystem : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI timeText;

    [Header("Start Time")]
    [Range(0, 23)] public int startHour = 7;
    [Range(0, 59)] public int startMinute = 30;

    [Header("Time Speed")]
    [Tooltip("1 = realtime, 2 = x2, 10 = x10")]
    public float timeScale = 2f;

    [Header("Debug")]
    public bool debugLogs = true;

    // ===== INTERNAL TIME =====
    private int hour;
    private int minute;
    private float secondAccumulator;

    // ===== PUBLIC READ ONLY =====
    public int Hour => hour;
    public int Minute => minute;

    /// <summary>
    /// Hour as float (ex: 7.5 = 7:30)
    /// </summary>
    public float CurrentHourFloat => hour + (minute / 60f);

    void Start()
    {
        hour = startHour;
        minute = startMinute;
        secondAccumulator = 0f;

        UpdateTimeText();

        Debug.Log($"[GameTimeSystem] START at {hour:D2}:{minute:D2} | timeScale={timeScale}");
    }

    void Update()
    {
        AdvanceTime();
    }

    void AdvanceTime()
    {
        float delta = Time.deltaTime * timeScale;
        secondAccumulator += delta;

        if (secondAccumulator < 60f)
            return;

        int minutesToAdd = Mathf.FloorToInt(secondAccumulator / 60f);
        secondAccumulator -= minutesToAdd * 60f;

        minute += minutesToAdd;

        if (minute >= 60)
        {
            int hoursToAdd = minute / 60;
            minute %= 60;
            hour += hoursToAdd;
        }

        if (hour >= 24)
        {
            hour %= 24;
        }

        UpdateTimeText();

        if (debugLogs)
        {
            Debug.Log($"[GameTimeSystem] Time updated → {hour:D2}:{minute:D2} ({CurrentHourFloat:0.00})");
        }
    }

    void UpdateTimeText()
    {
        if (!timeText)
        {
            Debug.LogWarning("[GameTimeSystem] TimeText is NULL");
            return;
        }

        bool isPM = hour >= 12;
        int displayHour = hour % 12;
        if (displayHour == 0) displayHour = 12;

        string suffix = isPM ? "PM" : "AM";
        timeText.text = $"{displayHour:D2}:{minute:D2} {suffix}";
    }
}
