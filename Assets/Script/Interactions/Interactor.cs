using UnityEngine;
using UnityEngine.InputSystem;

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

    // Nếu bạn muốn highlight bằng OutlineHighlighter từ Interactor
    private OutlineHighlighter currentOutline;

    private void Awake()
    {
        if (!cam) cam = GetComponentInChildren<Camera>();
        if (!cam) cam = Camera.main;
        SetUI(false, "");
    }

    private void Update()
    {
        FindTarget();
    }

	private void FindTarget()
	{
		if (!cam) return;

		Ray ray = new Ray(cam.transform.position, cam.transform.forward);
		if (debugDraw)
			Debug.DrawRay(ray.origin, ray.direction * distance, Color.yellow);

		var hits = Physics.RaycastAll(
			ray,
			distance,
			interactMask,
			QueryTriggerInteraction.Ignore
		);

		if (hits.Length == 0)
		{
			ClearFocusInternal();
			SetUI(false, "");
			return;
		}

		System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

		foreach (var hit in hits)
		{
			var interactable = FindInteractableInParents(hit.collider);
			if (interactable == null) continue;

			string hint = BuildHint(interactable);

			if (current != interactable)
			{
				ClearFocusInternal();

				current = interactable;
				currentGO = hit.collider.gameObject;

				currentOutline = hit.collider.GetComponentInParent<OutlineHighlighter>(true);
				if (currentOutline) currentOutline.SetHighlighted(true);

				current.OnFocus();
				SetUI(true, hint);
			}
			else
			{
				SetUI(true, hint);
			}

			return;
		}

		ClearFocusInternal();
		SetUI(false, "");
	}


    private IInteractable FindInteractableInParents(Collider col)
    {
        if (!col) return null;

        var behaviours = col.GetComponentsInParent<MonoBehaviour>(true);
        foreach (var b in behaviours)
        {
            if (b is IInteractable ii) return ii;
        }
        return null;
    }

    /// <summary>
    /// Tạo hint text:
    /// - Dòng 1: GetHintText() (E)
    /// - Dòng 2 (nếu có): GetAltHintText() (F) và CanAltInteract() == true
    /// </summary>
    private string BuildHint(IInteractable interactable)
    {
        if (interactable == null) return "";

        string hint = interactable.GetHintText();

        // Append hint cho phím F nếu object có hỗ trợ alt
        if (interactable is IAltInteractable alt && alt.CanAltInteract(this))
        {
            string altHint = alt.GetAltHintText();
            if (!string.IsNullOrEmpty(altHint))
                hint = string.IsNullOrEmpty(hint) ? altHint : $"{hint}\n{altHint}";
        }

        if (string.IsNullOrEmpty(hint))
            hint = "Ấn [E] để tương tác";

        return hint;
    }

    private void SetUI(bool canInteract, string hintText)
    {
        if (crosshair) crosshair.SetInteractable(canInteract);

        if (hintUI)
        {
            if (canInteract && !string.IsNullOrEmpty(hintText)) hintUI.Show(hintText);
            else hintUI.Hide();
        }
    }

    private void ClearFocusInternal()
    {
        if (current != null) current.OnUnfocus();
        if (currentOutline) currentOutline.SetHighlighted(false);

        current = null;
        currentGO = null;
        currentOutline = null;
    }

    // ===== INPUT =====
    // E
    public void OnInteract(InputValue value)
    {
        if (!value.isPressed) return;
        if (current == null) return;

        current.Interact(this);
        // Sau khi interact có thể đổi state (mở cửa, nhặt key...) -> update hint ngay
        SetUI(true, BuildHint(current));
    }

    // F
    public void OnAltInteract(InputValue value)
    {
        if (!value.isPressed) return;
        if (current == null) return;

        if (current is IAltInteractable alt && alt.CanAltInteract(this))
        {
            alt.AltInteract(this);
            // update hint ngay sau khi khóa/mở khóa
            SetUI(true, BuildHint(current));
        }
    }
	
	public InventoryBarUI inventoryUI;

	public void OnInventory(UnityEngine.InputSystem.InputValue value)
	{
		if (!value.isPressed) return;
		if (inventoryUI) inventoryUI.Toggle();
	}
}
