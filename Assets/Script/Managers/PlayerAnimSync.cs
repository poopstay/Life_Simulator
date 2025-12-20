using UnityEngine;

public class PlayerAnimSync : MonoBehaviour
{
    public Animator animator;
    public Rigidbody playerRb;
    public float walkThreshold = 0.1f;

    void Awake()
    {
        // Auto assign nếu bị None
        if (animator == null) animator = GetComponent<Animator>();
        if (playerRb == null) playerRb = GetComponentInParent<Rigidbody>();
    }

    void Update()
    {
        if (animator == null || playerRb == null) return;

        Vector3 v = playerRb.linearVelocity;
        float planarSpeed = new Vector3(v.x, 0f, v.z).magnitude;

        bool isWalking = planarSpeed > walkThreshold;
        animator.SetBool("isWalking", isWalking);
    }
}