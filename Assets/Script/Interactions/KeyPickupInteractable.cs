using UnityEngine;

public class KeyPickupInteractable : MonoBehaviour, IInteractable, IHintProvider
{
    public enum KeyType
    {
        MainDoor,
        Vehicle
    }

    [Header("Key Type")]
    public KeyType keyType = KeyType.MainDoor;

    [Header("Optional Highlight")]
    private OutlineHighlighter outline;

    [Header("Pickup")]
    public bool destroyOnPickup = true;

    [Header("Inventory Icon")]
    [Tooltip("Ảnh icon sẽ hiện trong inventory bar (UI).")]
    public Sprite itemIcon;

    [Tooltip("Tên hiển thị trong hint.")]
    public string itemName = "Chìa khóa";

    private void Awake()
    {
        outline = GetComponentInChildren<OutlineHighlighter>(true);
        if (!outline) outline = GetComponentInParent<OutlineHighlighter>(true);

        // Nếu bạn quên set name thì auto theo loại key
        if (string.IsNullOrWhiteSpace(itemName))
        {
            itemName = (keyType == KeyType.Vehicle) ? "Chìa khóa xe" : "Chìa khóa cửa";
        }
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

        // Cho key theo loại
        switch (keyType)
        {
            case KeyType.Vehicle:
                inv.GiveVehicleKey();
                break;

            default:
                inv.GiveMainDoorKey();
                break;
        }

        // Add icon vào UI inventory bar (nếu có)
        if (itemIcon != null && interactor.inventoryUI != null)
        {
            interactor.inventoryUI.AddItem(itemIcon);
        }
        else
        {
            if (itemIcon == null) Debug.LogWarning("KeyPickupInteractable: chưa gán itemIcon (Sprite).");
            if (interactor.inventoryUI == null) Debug.LogWarning("KeyPickupInteractable: Interactor chưa gán inventoryUI.");
        }

        if (outline) outline.SetHighlighted(false);

        if (destroyOnPickup) Destroy(gameObject);
        else gameObject.SetActive(false);
    }
}
