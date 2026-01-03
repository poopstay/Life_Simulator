using UnityEngine;

public class LightSwitchInteractable : MonoBehaviour, IInteractable
{
    public Light[] lights;

    [Header("Start State")]
    public bool startOn = false;
    private bool isOn;

    private OutlineHighlighter outline;

    private void Awake()
    {
        outline = GetComponentInChildren<OutlineHighlighter>(true);
        if (!outline) outline = GetComponentInParent<OutlineHighlighter>(true);

        isOn = startOn;
        ApplyLights();
    }

    void ApplyLights()
    {
        foreach (var l in lights)
            if (l) l.enabled = isOn;
    }

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
        isOn = !isOn;
        ApplyLights();
    }

    public string GetHintText()
    {
        return isOn ? "Ấn [E] để tắt đèn" : "Ấn [E] để bật đèn";
    }
}
