using UnityEngine;
using UnityEngine.InputSystem;
using System.Reflection;

public class Interactor : MonoBehaviour
{
    [Header("Raycast")]
    public Camera cam;
    public float distance = 2.2f;
    public LayerMask interactMask = ~0;
    public float sphereRadius = 0.12f;

    [Header("UI")]
    public CrosshairUI crosshair;
    public InteractHintUI hintUI;

    [Header("Debug")]
    public bool debugDraw = true;
    public bool logHitEveryFrame = false;
    public bool logStateChanges = true;

    private IInteractable current;
    private GameObject currentGO;
    private OutlineHighlighter currentOutline;

    void Awake()
    {
        if (!cam) cam = GetComponentInChildren<Camera>();
        if (!cam) cam = Camera.main;
        SetUI(false, "");
    }

    void Update()
    {
        FindTarget();
    }

    void FindTarget()
    {
        if (!cam) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (debugDraw)
            Debug.DrawRay(ray.origin, ray.direction * distance, Color.yellow);

        if (Physics.SphereCast(ray, sphereRadius, out RaycastHit hit, distance, interactMask, QueryTriggerInteraction.Ignore))
        {
            var hitGO = hit.collider.gameObject;

            if (logHitEveryFrame)
                Debug.Log($"HIT: {hit.collider.name} (layer={LayerMask.LayerToName(hitGO.layer)}) root={hit.collider.transform.root.name}");

            // ✅ Lấy IInteractable chắc chắn (duyệt MonoBehaviour rồi cast interface)
            IInteractable interactable = null;
            var behaviours = hit.collider.GetComponentsInParent<MonoBehaviour>(true);
            foreach (var b in behaviours)
            {
                if (b is IInteractable ii)
                {
                    interactable = ii;
                    break;
                }
            }

            if (interactable != null)
            {
                // ✅ Hint text: nếu script có GetHintText() thì lấy, không thì fallback
                string hint = GetHintTextSafe(interactable);

                if (hitGO != currentGO)
                {
                    ClearFocusInternal();

                    current = interactable;
                    currentGO = hitGO;

                    // Nếu bạn bỏ outline hoàn toàn thì có thể xoá block này
                    currentOutline = hit.collider.GetComponentInParent<OutlineHighlighter>(true);
                    if (currentOutline) currentOutline.SetHighlighted(true);

                    current.OnFocus();
                    SetUI(true, hint);

                    if (logStateChanges)
                        Debug.Log($"FOCUS -> {hit.collider.name} | interactable={current.GetType().Name}");
                }
                else
                {
                    // đang focus cùng object, vẫn update hint theo state (mở/đóng)
                    SetUI(true, hint);
                }

                return;
            }
        }

        if (current != null && logStateChanges)
            Debug.Log("UNFOCUS");

        ClearFocusInternal();
        SetUI(false, "");
    }

    // ✅ Tự động lấy hint nếu script có hàm:
    // public string GetHintText()
    string GetHintTextSafe(IInteractable interactable)
    {
        if (interactable == null) return "";

        // mặc định
        string fallback = "Ấn E để tương tác";

        // tìm method GetHintText() trên chính class (không cần interface)
        var type = interactable.GetType();
        MethodInfo mi = type.GetMethod("GetHintText", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (mi != null && mi.ReturnType == typeof(string) && mi.GetParameters().Length == 0)
        {
            try
            {
                var result = mi.Invoke(interactable, null) as string;
                if (!string.IsNullOrEmpty(result)) return result;
            }
            catch { /* ignore */ }
        }

        return fallback;
    }

    void SetUI(bool canInteract, string hintText)
    {
        if (crosshair) crosshair.SetInteractable(canInteract);

        if (hintUI)
        {
            if (canInteract && !string.IsNullOrEmpty(hintText)) hintUI.Show(hintText);
            else hintUI.Hide();
        }
    }

    void ClearFocusInternal()
    {
        if (current != null) current.OnUnfocus();
        if (currentOutline) currentOutline.SetHighlighted(false);

        current = null;
        currentGO = null;
        currentOutline = null;
    }

    public void OnInteract(InputValue value)
    {
        if (!value.isPressed) return;
        if (current == null) return;

        current.Interact(this);
    }
	
	public void OnAltInteract(InputValue value)
{
    if (!value.isPressed) return;
    if (current == null) return;

    // Chỉ Door mới xử lý AltInteract (F)
    if (current is IAltInteractable alt)
    {
        alt.AltInteract(this);
    }
}
}
