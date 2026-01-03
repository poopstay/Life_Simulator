using UnityEngine;

public interface IInteractable
{
    // tên hiển thị trên UI, ví dụ "Bật đèn", "Mở cửa"
    string Prompt { get; }

    // có cho tương tác không (tuỳ điều kiện quest, khoá cửa...)
    bool CanInteract(GameObject interactor);

    // thực hiện hành động
    void Interact(GameObject interactor);
}