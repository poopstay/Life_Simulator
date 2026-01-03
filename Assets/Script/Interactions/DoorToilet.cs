using UnityEngine;

public class DoorToilet : MonoBehaviour, IInteractable
{
    [Header("Door")]
    [Tooltip("Transform cánh cửa (PHẢI là object pivot ở bản lề).")]
    public Transform door;

    [Tooltip("Góc mở (theo trục Z). Ví dụ 90.")]
    public float openAngle = 90f;

    [Tooltip("Thời gian mở/đóng.")]
    public float duration = 0.5f;

    [Tooltip("Hướng mở: 1 hoặc -1.")]
    public float openDirection = 1f;

    [Header("Lift When Open")]
    public float openYOffset = 0.020f;

    [Header("Start State")]
    public bool startOpen = false;

    [Header("Optional Sound")]
    public AudioSource audioSource;
    public AudioClip openClip;
    public AudioClip closeClip;

    private bool isOpen;

    private float closedZ;
    private float closedY;

    private Coroutine anim;

    private void Awake()
    {
        if (!door) door = transform;

        // lưu trạng thái đóng
        closedZ = door.localEulerAngles.z;
        closedY = door.localPosition.y;

        isOpen = startOpen;
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
        float targetZ = open ? closedZ + openAngle * openDirection : closedZ;
        float targetY = open ? closedY + openYOffset : closedY;

        var e = door.localEulerAngles;
        e.z = targetZ;
        door.localEulerAngles = e;

        var p = door.localPosition;
        p.y = targetY;
        door.localPosition = p;
    }

    private System.Collections.IEnumerator AnimateDoor(bool open)
    {
        float startZ = NormalizeAngle(door.localEulerAngles.z);
        float endZ = NormalizeAngle(open ? closedZ + openAngle * openDirection : closedZ);

        float startY = door.localPosition.y;
        float endY = open ? closedY + openYOffset : closedY;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);

            float z = Mathf.LerpAngle(startZ, endZ, k);
            float y = Mathf.Lerp(startY, endY, k);

            var e = door.localEulerAngles;
            e.z = z;
            door.localEulerAngles = e;

            var p = door.localPosition;
            p.y = y;
            door.localPosition = p;

            yield return null;
        }

        SetDoorImmediate(open);
    }

    private float NormalizeAngle(float a)
    {
        a %= 360f;
        if (a < 0) a += 360f;
        return a;
    }
}
