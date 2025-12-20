using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathManager : MonoBehaviour
{
    public static DeathManager Instance { get; private set; }

    [Header("UI Fade (optional but recommended)")]
    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private float fadeDuration = 0.6f;

    [Header("Timing")]
    [SerializeField] private float delayBeforeFade = 0.4f;   // cho kịp thấy nhân vật ngã
    [SerializeField] private float delayBeforeReload = 0.6f; // sau khi fade xong

    private bool _isDying;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeCanvas)
        {
            fadeCanvas.alpha = 0f;
            fadeCanvas.blocksRaycasts = false;
            fadeCanvas.interactable = false;
        }
    }

    public void Die(GameObject player)
    {
        if (_isDying) return;
        _isDying = true;

        // 1) Khoá input / movement (tắt các script điều khiển)
        DisablePlayerControl(player);

        // 2) Bắn trigger isDie trên Animator
        var anim = player.GetComponent<PlayerAnimStateDriver>();
        if (anim) anim.Die();

        // 3) Fade + reload
        StartCoroutine(DieFlow());
    }

    private IEnumerator DieFlow()
    {
        yield return new WaitForSecondsRealtime(delayBeforeFade);

        if (fadeCanvas)
            yield return FadeToBlack();

        yield return new WaitForSecondsRealtime(delayBeforeReload);

        // Reload scene hiện tại (chơi lại từ đầu)
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);

        _isDying = false;
    }

    private IEnumerator FadeToBlack()
    {
        fadeCanvas.blocksRaycasts = true;

        float t = 0f;
        float start = fadeCanvas.alpha;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            fadeCanvas.alpha = Mathf.Lerp(start, 1f, t / fadeDuration);
            yield return null;
        }

        fadeCanvas.alpha = 1f;
    }

    private void DisablePlayerControl(GameObject player)
    {
        // Tắt toàn bộ MonoBehaviour trừ những thứ cần thiết như Animator/Audio
        // Bạn có thể tinh chỉnh danh sách này theo controller bạn dùng.
        foreach (var mb in player.GetComponents<MonoBehaviour>())
        {
            // giữ lại driver animation (để còn phát Dying)
            if (mb is PlayerAnimStateDriver) continue;
            mb.enabled = false;
        }

        // Dừng rigidbody để không trượt/phi nữa
        var rb = player.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }
}
