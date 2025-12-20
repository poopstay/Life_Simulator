using UnityEngine;

public class LookAroundDriver : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float mouseThreshold = 0.15f;

    void Reset()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (!animator) return;

        // Nếu crouch thì tắt lookaround
        if (animator.GetBool("isCrouch"))
        {
            animator.SetBool("isLookAround", false);
            return;
        }

        // Nếu đang đi/chạy thì tắt lookaround
        if (animator.GetBool("isWalking") || animator.GetBool("isRun"))
        {
            animator.SetBool("isLookAround", false);
            return;
        }

        float mx = Mathf.Abs(Input.GetAxisRaw("Mouse X"));
        float my = Mathf.Abs(Input.GetAxisRaw("Mouse Y"));

        bool movedMouse = (mx + my) >= mouseThreshold;
        animator.SetBool("isLookAround", movedMouse);
    }
}
