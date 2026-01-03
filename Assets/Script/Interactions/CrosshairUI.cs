using UnityEngine;
using UnityEngine.UI;

public class CrosshairUI : MonoBehaviour
{
    [SerializeField] private Image img;
    [SerializeField] private Color normal = Color.white;
    [SerializeField] private Color interact = Color.green;

    private void Reset()
    {
        img = GetComponent<Image>();
    }

    public void SetInteractable(bool canInteract)
    {
        if (!img) return;
        img.color = canInteract ? interact : normal;
    }
}
