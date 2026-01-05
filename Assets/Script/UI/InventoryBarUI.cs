using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryBarUI : MonoBehaviour
{
    [Header("Layout")]
    [Range(0.05f, 0.15f)]
    public float widthPercent = 0.08f;      // 8% màn hình (5-10% theo yêu cầu)
    public float slideDuration = 0.18f;     // tốc độ trượt
    public float rightPadding = 0f;        // cách mép phải 1 chút

    [Header("Row Container (ItemsRow)")]
    public RectTransform rowRoot;

    [Header("Item Icon Settings")]
    public float iconSize = 100f;
    public float iconPadding = 6f;

    private RectTransform panel;
    private CanvasGroup cg;

    private bool isVisible = true;
    private Coroutine slideCo;

    // lưu list icon (Sprite) hiện có
    private readonly List<Sprite> items = new();

    void Awake()
    {
        panel = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>();
        if (!cg) cg = gameObject.AddComponent<CanvasGroup>();

        if (!rowRoot)
        {
            var found = transform.Find("ItemsRow");
            if (found) rowRoot = found.GetComponent<RectTransform>();
        }

        ApplyResponsiveWidth();
        SetVisibleInstant(false); // mặc định ẩn (bạn muốn hiện luôn thì đổi true)
    }

    void OnRectTransformDimensionsChange()
    {
        // tự cập nhật khi đổi resolution
        ApplyResponsiveWidth();
        SnapToState();
    }

    void ApplyResponsiveWidth()
    {
        if (!panel) return;
        float w = Screen.width * widthPercent;
        panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
    }

    // ===== Public API =====

    public void Toggle()
    {
        SetVisible(!isVisible);
    }

    public void SetVisible(bool show)
    {
        isVisible = show;

        if (slideCo != null) StopCoroutine(slideCo);
        slideCo = StartCoroutine(SlideRoutine(show));
    }

    public void SetVisibleInstant(bool show)
    {
        isVisible = show;
        if (slideCo != null) StopCoroutine(slideCo);
        slideCo = null;

        cg.alpha = show ? 1f : 0f;
        cg.blocksRaycasts = show;
        cg.interactable = show;

        SnapToState();
    }

    public void AddItem(Sprite icon)
    {
        if (!icon) return;
        items.Add(icon);
        RebuildRow();
    }

    public void Clear()
    {
        items.Clear();
        RebuildRow();
    }

    // ===== Internals =====

    void RebuildRow()
    {
        if (!rowRoot) return;

        // clear children
        for (int i = rowRoot.childCount - 1; i >= 0; i--)
            Destroy(rowRoot.GetChild(i).gameObject);

        // create icons
        foreach (var sp in items)
        {
            var go = new GameObject("ItemIcon", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(rowRoot, false);

            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(iconSize, iconSize);

            var img = go.GetComponent<Image>();
            img.sprite = sp;
            img.preserveAspect = true;

            // padding bằng LayoutElement
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = iconSize + iconPadding;
            le.preferredHeight = iconSize + iconPadding;
        }
    }

    IEnumerator SlideRoutine(bool show)
    {
        // vị trí: panel anchor right, pivot 1 -> chỉ cần thay anchoredPosition.x
        float w = panel.rect.width;
        float xShown = -rightPadding;        // hơi thò vào màn hình (âm vì pivot right)
        float xHidden = w + rightPadding;    // đẩy ra ngoài phải

        float start = panel.anchoredPosition.x;
        float end = show ? xShown : xHidden;

        float t = 0f;
        float dur = Mathf.Max(0.01f, slideDuration);

        // alpha “nhẹ nhàng”
        float a0 = cg.alpha;
        float a1 = show ? 1f : 0f;

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / dur);

            panel.anchoredPosition = new Vector2(Mathf.Lerp(start, end, k), panel.anchoredPosition.y);
            cg.alpha = Mathf.Lerp(a0, a1, k);

            yield return null;
        }

        panel.anchoredPosition = new Vector2(end, panel.anchoredPosition.y);
        cg.alpha = a1;
        cg.blocksRaycasts = show;
        cg.interactable = show;

        slideCo = null;
    }

    void SnapToState()
    {
        if (!panel) return;

        float w = panel.rect.width;
        float xShown = -rightPadding;
        float xHidden = w + rightPadding;

        float x = isVisible ? xShown : xHidden;
        panel.anchoredPosition = new Vector2(x, panel.anchoredPosition.y);
    }
}
