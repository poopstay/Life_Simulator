using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VehicleDoorInteractable : MonoBehaviour, IInteractable, IHintProvider
{
    [Header("Requirement")]
    public string requiredKeyName = "Chìa khóa xe";

    [Header("Spawn / Store")]
    [Tooltip("Nếu không dùng existingVehicleInScene thì sẽ Instantiate từ prefab này.")]
    public GameObject vehiclePrefab;

    [Tooltip("Điểm spawn/điểm đặt xe.")]
    public Transform spawnPoint;

    [Tooltip("Nếu scene đã có sẵn 1 xe (Vehicle) thì kéo vào đây. Khi lấy/cất sẽ bật/tắt xe này thay vì Instantiate/Destroy.")]
    public GameObject existingVehicleInScene;

    [Tooltip("Nếu không dùng existingVehicleInScene: khi cất xe sẽ SetActive(false) thay vì Destroy.")]
    public bool storeByDeactivating = false;

    [Tooltip("Nâng xe lên một chút khi spawn để tránh xuyên nền.")]
    public float spawnUpOffset = 0.1f;

    [Header("Fade")]
    [Tooltip("CanvasGroup của FadePanel (panel đen fullscreen).")]
    public CanvasGroup fadeGroup;
    public float fadeDuration = 0.6f;
    public float holdBlackSeconds = 0.4f;

    [Header("Lock Input During Fade")]
    [Tooltip("Nếu có PlayerInputLocker trên player, hệ sẽ tự Lock/Unlock trong lúc fade.")]
    public bool lockPlayerInputDuringFade = true;

    [Header("UI Message (optional)")]
    [Tooltip("TMP_Text để báo 'cần chìa khóa'. Nếu không gán sẽ Debug.Log.")]
    public TMP_Text messageLabel;
    public float messageSeconds = 1.2f;

    // State
    private bool isBusy;                // đang chạy sequence (fade/spawn/store)
    private bool isVehicleOut;          // xe đang ở ngoài (đã lấy ra)
    private GameObject spawnedVehicleInstance; // instance spawn nếu dùng prefab

    // Cache refs
    private PlayerInventory cachedInventory;
    private PlayerInputLocker inputLocker;

    // =========================
    // Inventory + Locker
    // =========================
    private void CacheRefs(Interactor interactor)
    {
        // Inventory
        if (cachedInventory == null)
        {
            if (interactor)
            {
                cachedInventory = interactor.GetComponentInParent<PlayerInventory>(true);
                if (!cachedInventory) cachedInventory = interactor.GetComponent<PlayerInventory>();
            }

            if (cachedInventory == null)
                cachedInventory = FindObjectOfType<PlayerInventory>();
        }

        // InputLocker (optional)
        if (inputLocker == null)
        {
            if (interactor)
            {
                inputLocker = interactor.GetComponentInParent<PlayerInputLocker>(true);
                if (!inputLocker) inputLocker = interactor.GetComponent<PlayerInputLocker>();
            }

            if (inputLocker == null)
                inputLocker = FindObjectOfType<PlayerInputLocker>();
        }
    }

    private void Awake()
    {
        // Nếu bạn gán existingVehicleInScene và nó đang active ngay từ đầu => coi như xe đang "out"
        if (existingVehicleInScene && existingVehicleInScene.activeInHierarchy)
            isVehicleOut = true;

        // Nếu bạn không dùng existing nhưng trong editor bạn đã spawn sẵn instance
        if (!existingVehicleInScene && spawnedVehicleInstance && spawnedVehicleInstance.activeInHierarchy)
            isVehicleOut = true;
    }

    // =========================
    // Hint
    // =========================
    public string GetHintText()
    {
        // Không có inventory cache -> coi như chưa có chìa để hiển thị hint “cần chìa”
        if (cachedInventory == null || !cachedInventory.HasVehicleKey)
            return $"Cần [{requiredKeyName}] để lấy xe";

        return isVehicleOut ? "Ấn [E] để cất xe" : "Ấn [E] để lấy xe";
    }

    // =========================
    // IInteractable
    // =========================
    public void OnFocus()
    {
        // cố gắng cache sớm để hint cập nhật đúng
        if (cachedInventory == null) cachedInventory = FindObjectOfType<PlayerInventory>();
        if (inputLocker == null) inputLocker = FindObjectOfType<PlayerInputLocker>();

        SyncVehicleOutState();
    }

    public void OnUnfocus()
    {
        // clear để lần sau lấy trạng thái mới nhất
        cachedInventory = null;
    }

    public void Interact(Interactor interactor)
    {
        if (isBusy) return;

        CacheRefs(interactor);

        if (!cachedInventory)
        {
            Debug.LogWarning("[VehicleDoor] Không tìm thấy PlayerInventory.");
            return;
        }

        if (!cachedInventory.HasVehicleKey)
        {
            ShowMessage($"Cần [{requiredKeyName}] để lấy xe");
            return;
        }

        // Toggle lấy/cất
        if (!isVehicleOut)
        {
            if (!spawnPoint)
            {
                Debug.LogWarning("[VehicleDoor] Chưa gán spawnPoint (hãy tạo Empty 'VehicleSpawnPoint' và kéo vào).");
                return;
            }

            if (!existingVehicleInScene && !vehiclePrefab)
            {
                Debug.LogWarning("[VehicleDoor] Chưa gán vehiclePrefab hoặc existingVehicleInScene.");
                return;
            }

            StartCoroutine(TakeVehicleSequence());
        }
        else
        {
            StartCoroutine(StoreVehicleSequence());
        }
    }

    // =========================
    // Sequences
    // =========================
    private IEnumerator TakeVehicleSequence()
    {
        isBusy = true;

        // Lock input
        if (lockPlayerInputDuringFade && inputLocker) inputLocker.Lock();

        // Fade to black
        if (fadeGroup) yield return Fade(1f);
        if (holdBlackSeconds > 0f) yield return new WaitForSeconds(holdBlackSeconds);

        // Take (spawn/enable)
        if (existingVehicleInScene)
        {
            existingVehicleInScene.transform.SetPositionAndRotation(
                spawnPoint.position + Vector3.up * spawnUpOffset,
                spawnPoint.rotation
            );
            existingVehicleInScene.SetActive(true);
            isVehicleOut = true;
        }
        else
        {
            Vector3 pos = spawnPoint.position + Vector3.up * spawnUpOffset;
            Quaternion rot = spawnPoint.rotation;

            spawnedVehicleInstance = Instantiate(vehiclePrefab, pos, rot);
            isVehicleOut = true;
        }

        // Fade back
        if (fadeGroup) yield return Fade(0f);

        // Unlock input
        if (lockPlayerInputDuringFade && inputLocker) inputLocker.Unlock();

        isBusy = false;
    }

    private IEnumerator StoreVehicleSequence()
    {
        isBusy = true;

        // Lock input
        if (lockPlayerInputDuringFade && inputLocker) inputLocker.Lock();

        // Fade to black
        if (fadeGroup) yield return Fade(1f);
        if (holdBlackSeconds > 0f) yield return new WaitForSeconds(holdBlackSeconds);

        // Store (disable/destroy)
        if (existingVehicleInScene)
        {
            existingVehicleInScene.SetActive(false);
            isVehicleOut = false;
        }
        else
        {
            if (spawnedVehicleInstance)
            {
                if (storeByDeactivating) spawnedVehicleInstance.SetActive(false);
                else Destroy(spawnedVehicleInstance);

                spawnedVehicleInstance = null;
            }
            isVehicleOut = false;
        }

        // Fade back
        if (fadeGroup) yield return Fade(0f);

        // Unlock input
        if (lockPlayerInputDuringFade && inputLocker) inputLocker.Unlock();

        isBusy = false;
    }

    private void SyncVehicleOutState()
    {
        if (existingVehicleInScene)
        {
            isVehicleOut = existingVehicleInScene.activeInHierarchy;
            return;
        }

        if (spawnedVehicleInstance)
            isVehicleOut = spawnedVehicleInstance.activeInHierarchy;
        else
            isVehicleOut = false;
    }

    // =========================
    // Fade
    // =========================
    private IEnumerator Fade(float targetAlpha)
    {
        if (!fadeGroup) yield break;

        // khi fade to black thì chặn raycast, khi fade về 0 thì thả
        fadeGroup.blocksRaycasts = true;
        fadeGroup.interactable = false;

        float start = fadeGroup.alpha;
        float t = 0f;
        float dur = Mathf.Max(0.01f, fadeDuration);

        while (t < dur)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / dur);
            fadeGroup.alpha = Mathf.Lerp(start, targetAlpha, k);
            yield return null;
        }

        fadeGroup.alpha = targetAlpha;

        if (Mathf.Approximately(targetAlpha, 0f))
            fadeGroup.blocksRaycasts = false;
    }

    // =========================
    // Message
    // =========================
    private void ShowMessage(string msg)
    {
        if (messageLabel)
        {
            StopAllCoroutines();
            StartCoroutine(ShowMessageRoutine(msg));
        }
        else
        {
            Debug.Log("[VehicleDoor] " + msg);
        }
    }

    private IEnumerator ShowMessageRoutine(string msg)
    {
        messageLabel.gameObject.SetActive(true);
        messageLabel.text = msg;
        yield return new WaitForSeconds(messageSeconds);
        messageLabel.text = "";
        messageLabel.gameObject.SetActive(false);
    }
}
