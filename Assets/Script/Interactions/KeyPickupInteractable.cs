using UnityEngine;

public class KeyPickupInteractable : MonoBehaviour, IInteractable
{
    [Header("Optional Highlight")]
    private OutlineHighlighter outline;

    [Header("Pickup")]
    public bool destroyOnPickup = true;

    [Header("Inventory Icon")]
    [Tooltip("Ảnh icon sẽ hiện trong inventory bar (UI).")]
    public Sprite itemIcon;

    [Tooltip("Tên hiển thị trong hint.")]
    public string itemName = "Chìa khóa cửa";

    private void Awake()
    {
        outline = GetComponentInChildren<OutlineHighlighter>(true);
        if (!outline) outline = GetComponentInParent<OutlineHighlighter>(true);
    }

    public string GetHintText() => $"Ấn [E] để nhặt [{itemName}]";

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
        if (!interactor) return;

        // Tìm inventory trên player
        var inv = interactor.GetComponentInParent<PlayerInventory>(true);
        if (!inv) inv = interactor.GetComponent<PlayerInventory>();

        if (!inv)
        {
            Debug.LogWarning("Không tìm thấy PlayerInventory trên interactor/player.");
            return;
        }

        // Cho chìa
        inv.GiveMainDoorKey();

        // Add icon vào UI inventory bar (nếu có)
        if (itemIcon != null && interactor.inventoryUI != null)
        {
            interactor.inventoryUI.AddItem(itemIcon);
        }
        else
        {
            // Không bắt buộc, chỉ để bạn biết thiếu set reference/icon
            if (itemIcon == null) Debug.LogWarning("KeyPickupInteractable: chưa gán itemIcon (Sprite).");
            if (interactor.inventoryUI == null) Debug.LogWarning("KeyPickupInteractable: Interactor chưa gán inventoryUI.");
        }

        // Tắt highlight nếu có
        if (outline) outline.SetHighlighted(false);

        if (destroyOnPickup) Destroy(gameObject);
        else gameObject.SetActive(false);
    }
}
