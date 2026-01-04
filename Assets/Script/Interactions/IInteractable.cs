using UnityEngine;

public interface IInteractable
{
    /// <summary>
    /// Text hiển thị khi nhìn vào object
    /// </summary>
    string GetHintText();

    /// <summary>
    /// Gọi khi nhấn phím tương tác (E)
    /// </summary>
    void Interact(Interactor interactor);

    /// <summary>
    /// Gọi khi player nhìn vào object
    /// </summary>
    void OnFocus();

    /// <summary>
    /// Gọi khi player rời mắt khỏi object
    /// </summary>
    void OnUnfocus();
}
