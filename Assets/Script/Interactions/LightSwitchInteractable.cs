using UnityEngine;

public class LightSwitchInteractable : MonoBehaviour, IInteractable
{
    [Header("Light to toggle")]
    public Light targetLight;

    [Header("Highlight")]
    public Renderer rend;
    public float emissionIntensity = 2.0f;

    MaterialPropertyBlock mpb;
    bool focused;

    void Awake()
    {
        if (!rend) rend = GetComponentInChildren<Renderer>();
        mpb = new MaterialPropertyBlock();
    }

    public void OnFocus()
    {
        focused = true;
        SetEmission(true);
    }

    public void OnUnfocus()
    {
        focused = false;
        SetEmission(false);
    }

    public void Interact(Interactor interactor)
    {
        if (targetLight) targetLight.enabled = !targetLight.enabled;
    }

    void SetEmission(bool on)
    {
        if (!rend) return;

        rend.GetPropertyBlock(mpb);
        // URP Lit dùng _EmissionColor
        mpb.SetColor("_EmissionColor", on ? Color.white * emissionIntensity : Color.black);
        rend.SetPropertyBlock(mpb);

        // Lưu ý: material phải bật Emission trong shader (URP/Lit) thì mới thấy rõ.
    }
}
