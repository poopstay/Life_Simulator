using UnityEngine;

public class DoorInteractable : MonoBehaviour, IInteractable
{
    [Header("Door")]
    [Tooltip("Transform cánh cửa (PHẢI là object pivot ở bản lề).")]
    public Transform door;

    [Tooltip("Nếu cửa xoay theo trục Z (thường cửa toilet), bật cái này.")]
    public bool rotateZ = false;

    [Tooltip("Góc mở. Ví dụ 90.")]
    public float openAngle = 90f;

    [Tooltip("Thời gian mở/đóng.")]
    public float duration = 0.5f;

    [Tooltip("Hướng mở: 1 hoặc -1.")]
    public float openDirection = 1f;

    [Header("Start State")]
    public bool startOpen = false;

    [Header("Optional Highlight")]
    private OutlineHighlighter outline;

    [Header("Optional Sound")]
    public AudioSource audioSource;
    public AudioClip openClip;
    public AudioClip closeClip;

    private bool isOpen;
    private float closedAngle;     // angle khi đóng (Y hoặc Z)
    private Coroutine anim;

    private void Awake()
    {
        if (!door) door = transform;

        outline = GetComponentInChildren<OutlineHighlighter>(true);
        if (!outline) outline = GetComponentInParent<OutlineHighlighter>(true);

        // Lưu góc đóng
        closedAngle = rotateZ ? door.localEulerAngles.z : door.localEulerAngles.y;

        isOpen = startOpen;
        SetAngleImmediate(isOpen ? closedAngle + openAngle * openDirection : closedAngle);
    }

    // ===== IInteractable (Interactor-system) =====
    public void OnFocus()
    {
        if (outline) outline.SetHighlighted(true);
    }

    public void OnUnfocus()
    {
        if (outline) outline.SetHighlighted(false);
    }

    public void Interact(Interactor interactor)
    {
        isOpen = !isOpen;

        if (anim != null) StopCoroutine(anim);

        float target = isOpen
            ? closedAngle + openAngle * openDirection
            : closedAngle;

        anim = StartCoroutine(AnimateTo(target));

        if (audioSource)
        {
            var clip = isOpen ? openClip : closeClip;
            if (clip) audioSource.PlayOneShot(clip, 0.8f);
        }
    }

    public string GetHintText()
    {
        return isOpen ? "Ấn [E] để đóng cửa" : "Ấn [E] để mở cửa";
    }

    // ===== Helpers =====
    private System.Collections.IEnumerator AnimateTo(float targetAngle)
    {
        float start = GetAngle();
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / Mathf.Max(0.0001f, duration));
            float a = Mathf.LerpAngle(start, targetAngle, k);
            SetAngleImmediate(a);
            yield return null;
        }

        SetAngleImmediate(targetAngle);
        anim = null;
    }

    private float GetAngle()
        => rotateZ ? door.localEulerAngles.z : door.localEulerAngles.y;

    private void SetAngleImmediate(float angle)
    {
        Vector3 e = door.localEulerAngles;
        if (rotateZ) e.z = angle;
        else e.y = angle;
        door.localEulerAngles = e;
    }
}
