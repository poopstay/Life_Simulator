using UnityEngine;

public class PlayerInputLocker : MonoBehaviour
{
    [Header("References")]
    public MonoBehaviour[] movementScripts;   // script di chuyển
    public MonoBehaviour[] lookScripts;        // script xoay camera
    public MonoBehaviour[] interactScripts;    // Interactor, input E

    private bool locked;

    public void Lock()
    {
        if (locked) return;
        locked = true;

        SetEnabled(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Unlock()
    {
        if (!locked) return;
        locked = false;

        SetEnabled(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SetEnabled(bool value)
    {
        foreach (var s in movementScripts)
            if (s) s.enabled = value;

        foreach (var s in lookScripts)
            if (s) s.enabled = value;

        foreach (var s in interactScripts)
            if (s) s.enabled = value;
    }
}
