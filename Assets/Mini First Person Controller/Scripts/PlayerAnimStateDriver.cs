using UnityEngine;

public class PlayerAnimStateDriver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody rb;

    [Header("Tuning")]
    [SerializeField] private float walkThreshold = 0.1f;
    [SerializeField] private float runSpeedThreshold = 2.2f;
    [SerializeField] private float forwardRunMin = 0.15f; // cấm chạy lùi

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

        Vector3 v = rb.linearVelocity; v.y = 0f;

        bool moving = v.magnitude > walkThreshold;

        // không cho chạy lùi
        Vector3 local = transform.InverseTransformDirection(v);
        bool movingForward = local.z > forwardRunMin;

        bool running = moving && movingForward && v.magnitude > runSpeedThreshold;

        animator.SetBool(IsWalking, moving);
        animator.SetBool(IsRun, running);
    }

    public void PlayJump()
    {
        if (isDead) return;
        animator.ResetTrigger(IsJump);
        animator.SetTrigger(IsJump);
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        animator.ResetTrigger(IsDie);
        animator.SetTrigger(IsDie);
    }
}
