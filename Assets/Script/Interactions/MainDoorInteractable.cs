using UnityEngine;

public class MainDoorInteractable : MonoBehaviour, IInteractable, IAltInteractable
{
    [Header("Door")]
    public Transform door;
    public bool rotateZ = false;
    public float openAngle = 90f;
    public float duration = 0.5f;
    public float openDirection = 1f;

    [Header("Lock State")]
    public bool startLocked = true;

    [Header("Start Open")]
    public bool startOpen = false;

    [Header("Optional Highlight")]
    private OutlineHighlighter outline;

    private bool isOpen;
    private bool isLocked;
    private float closedAngle;
    private Coroutine anim;

    // Cache trạng thái có chìa để GetHintText() hiển thị đúng (1 dòng)
    private bool hasKeyCached = false;

    private void Awake()
    {
        if (!door) door = transform;

        outline = GetComponentInChildren<OutlineHighlighter>(true);
        if (!outline) outline = GetComponentInParent<OutlineHighlighter>(true);

        closedAngle = rotateZ ? door.localEulerAngles.z : door.localEulerAngles.y;

        isLocked = startLocked;
        isOpen = startOpen && !isLocked; // nếu locked thì ép đóng

        SetAngleImmediate(isOpen ? closedAngle + openAngle * openDirection : closedAngle);
    }

    // ===== Helpers =====
    private bool HasKey(Interactor interactor)
    {
        if (!interactor) return false;

        var inv = interactor.GetComponentInParent<PlayerInventory>(true);
        if (!inv) inv = interactor.GetComponent<PlayerInventory>();

        return inv && inv.HasMainDoorKey;
    }

    // ===== IInteractable (E) =====
    public string GetHintText()
    {
        // 1 dòng duy nhất
        if (isLocked)
        {
            return hasKeyCached
                ? "Cửa đang khóa – Ấn [F] để mở khóa"
                : "Cửa đang khóa – Cần tìm [Chìa khóa cửa]";
        }

        return isOpen
            ? "Ấn [E] để đóng cửa – Ấn [F] để khóa cửa"
            : "Ấn [E] để mở cửa – Ấn [F] để khóa cửa";
    }

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
        // E: mở/đóng cửa, nhưng nếu locked thì chặn
        if (isLocked) return;

        isOpen = !isOpen;

        float target = isOpen
            ? closedAngle + openAngle * openDirection
            : closedAngle;

        if (anim != null) StopCoroutine(anim);
        anim = StartCoroutine(AnimateTo(target));
    }

    // ===== IAltInteractable (F) =====
    // Hint alt không dùng nữa vì bạn muốn 1 dòng (đã gộp vào GetHintText)
    public string GetAltHintText() => "";

    public bool CanAltInteract(Interactor interactor)
    {
        // cập nhật cache để GetHintText hiển thị đúng
        hasKeyCached = HasKey(interactor);
        return hasKeyCached;
    }

    public void AltInteract(Interactor interactor)
    {
        if (!HasKey(interactor))
        {
            hasKeyCached = false;
            return;
        }

        hasKeyCached = true;

        // Nếu đang mở mà khóa -> ép đóng trước rồi khóa
        if (!isLocked)
        {
            if (isOpen)
            {
                isOpen = false;
                if (anim != null) StopCoroutine(anim);
                anim = StartCoroutine(AnimateTo(closedAngle));
            }
            isLocked = true;
        }
        else
        {
            // đang locked -> mở khóa
            isLocked = false;
        }
    }

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
