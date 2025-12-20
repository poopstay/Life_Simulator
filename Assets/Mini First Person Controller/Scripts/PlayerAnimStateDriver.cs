using UnityEngine;

public class PlayerAnimStateDriver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody rb;

    [Header("Tuning")]
    [SerializeField] private float walkThreshold = 0.1f;

    [Tooltip("Giữ Shift để chạy")]
    [SerializeField] private KeyCode runKey = KeyCode.LeftShift;

    [Tooltip("Ngưỡng input để tránh drift (tay cầm/axis)")]
    [SerializeField] private float inputDeadzone = 0.1f;

    [Tooltip("Cấm chạy lùi (khi nhấn S/Vertical < 0)")]
    [SerializeField] private bool forbidBackwardRun = true;

    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private static readonly int IsRun     = Animator.StringToHash("isRun");
    private static readonly int IsJump    = Animator.StringToHash("isJump"); // Trigger
    private static readonly int IsDie     = Animator.StringToHash("isDie");  // Trigger

    private bool isDead;

    private void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!rb) rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (isDead || animator == null || rb == null) return;

        // 1) Moving theo vận tốc thật
        Vector3 v = rb.linearVelocity;
        v.y = 0f;
        bool moving = v.magnitude > walkThreshold;

        // 2) Input để xác định hướng + có đang muốn chạy không
        float h = Input.GetAxisRaw("Horizontal");
        float ver = Input.GetAxisRaw("Vertical");
        Vector2 input = new Vector2(h, ver);

        bool hasInput = input.sqrMagnitude > (inputDeadzone * inputDeadzone);
        bool runPressed = Input.GetKey(runKey);

        // 3) Cấm chạy lùi: chỉ chặn khi người chơi đang nhấn lùi (ver < 0)
        bool backwardInput = ver < -inputDeadzone;
        bool allowRunByDirection = !forbidBackwardRun || !backwardInput;

        // ✅ RUN: Shift + có input (kể cả A/D) + đang thực sự di chuyển + (không lùi nếu bật cấm)
        bool running = moving && hasInput && runPressed && allowRunByDirection;

        animator.SetBool(IsWalking, moving);
        animator.SetBool(IsRun, running);
    }

    public void PlayJump()
    {
        if (isDead || animator == null) return;
        animator.ResetTrigger(IsJump);
        animator.SetTrigger(IsJump);
    }

    public void Die()
    {
        if (isDead || animator == null) return;
        isDead = true;
        animator.ResetTrigger(IsDie);
        animator.SetTrigger(IsDie);
    }
}
