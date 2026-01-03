using UnityEngine;

public interface IInteractable
{
    // gọi khi camera đang nhìn trúng object (để bật highlight)
    void OnFocus();

    // gọi khi camera rời khỏi object (tắt highlight)
    void OnUnfocus();

    // gọi khi nhấn E
    void Interact(Interactor interactor);
}
