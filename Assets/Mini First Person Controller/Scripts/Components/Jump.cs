using UnityEngine;

public class Jump : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Jump")]
    public float jumpStrength = 2f;
    public event System.Action Jumped;

    [SerializeField, Tooltip("Prevents jumping when the transform is in mid-air.")]
    private GroundCheck groundCheck;

    [Header("Animation")]
    [SerializeField] private PlayerAnimStateDriver animDriver;

    void Reset()
    {
        // Auto find GroundCheck
        groundCheck = GetComponentInChildren<GroundCheck>();
        animDriver = GetComponent<PlayerAnimStateDriver>();
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (!groundCheck)
            groundCheck = GetComponentInChildren<GroundCheck>();

        if (!animDriver)
            animDriver = GetComponent<PlayerAnimStateDriver>();
    }

    void Update()
    {
        // Jump when Jump button is pressed and grounded
        if (Input.GetButtonDown("Jump") && (!groundCheck || groundCheck.isGrounded))
        {
            DoJump();
        }
    }

	private void DoJump()
	{
		// reset vận tốc Y để không cộng dồn
		Vector3 v = rb.linearVelocity;
		v.y = 0f;
		rb.linearVelocity = v;

		// nhảy vừa phải (tune jumpStrength khoảng 4–7)
		rb.AddForce(Vector3.up * jumpStrength, ForceMode.Impulse);

		animDriver?.PlayJump();
		Jumped?.Invoke();
	}
}
