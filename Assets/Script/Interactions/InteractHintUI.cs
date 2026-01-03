using UnityEngine;
using TMPro;

public class InteractHintUI : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    private void Awake()
    {
        if (!label) label = GetComponent<TMP_Text>();
    }

    public void Show(string text)
    {
        if (!label) return;
        label.text = text;
        if (!gameObject.activeSelf) gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (!label) return;
        label.text = "";
        if (gameObject.activeSelf) gameObject.SetActive(false);
    }
}
