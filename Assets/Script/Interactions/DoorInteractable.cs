using UnityEngine;

public class DoorInteractable : MonoBehaviour, IInteractable
{
    [Header("Door")]
    [Tooltip("Transform cánh cửa cần quay (thường là object Door).")]
    public Transform door;

    [Tooltip("Góc mở (Y) so với góc đóng. Ví dụ 90 độ.")]
    public float openAngle = 80f;

    [Tooltip("Thời gian mở/đóng.")]
    public float duration = 0.5f;

    [Tooltip("Mở theo chiều nào. 1 hoặc -1 để đổi hướng.")]
    public float openDirection = 1f;

    [Header("Start State")]
    public bool startOpen = false;

    [Header("Optional Sound")]
    public AudioSource audioSource;
    public AudioClip openClip;
    public AudioClip closeClip;

    private bool isOpen;
    private float closedY;
    private Coroutine anim;

    private void Awake()
    {
        if (!door) door = transform.root; // fallback (tốt nhất vẫn kéo tay)
        closedY = door.localEulerAngles.y;
        isOpen = startOpen;

        // set trạng thái ban đầu
        SetDoorImmediate(isOpen);
    }

    public void OnFocus() { }
    public void OnUnfocus() { }

    public void Interact(Interactor interactor)
    {
        isOpen = !isOpen;

        if (anim != null) StopCoroutine(anim);
        anim = StartCoroutine(AnimateDoor(isOpen));

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

    private void SetDoorImmediate(bool open)
    {
        float targetY = open ? closedY + openAngle * openDirection : closedY;
        var e = door.localEulerAngles;
        e.y = targetY;
        door.localEulerAngles = e;
    }

    private System.Collections.IEnumerator AnimateDoor(bool open)
    {
        float startY = door.localEulerAngles.y;
        float endY = open ? closedY + openAngle * openDirection : closedY;

        // xử lý wrap 0..360 cho mượt
        startY = NormalizeAngle(startY);
        endY = NormalizeAngle(endY);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            float y = Mathf.LerpAngle(startY, endY, k);

            var e = door.localEulerAngles;
            e.y = y;
            door.localEulerAngles = e;

            yield return null;
        }

        var ee = door.localEulerAngles;
        ee.y = endY;
        door.localEulerAngles = ee;
    }

    private float NormalizeAngle(float a)
    {
        a %= 360f;
        if (a < 0) a += 360f;
        return a;
    }
}
