using UnityEngine;

public class Crouch : MonoBehaviour
{
    [Header("Input")]
    public KeyCode key = KeyCode.LeftControl;

    [Tooltip("True = bấm 1 lần crouch/bấm lại đứng. False = giữ để crouch.")]
    public bool toggle = true;

    [Header("Animator")]
    [Tooltip("Animator của nhân vật (Ch31_nonPBR Animator).")]
    public Animator animator;
    public string crouchBoolName = "isCrouch";

    [Header("Slow Movement")]
    [Tooltip("Movement to slow down when crouched.")]
    public FirstPersonMovement movement;
    [Tooltip("Movement speed when crouched.")]
    public float movementSpeed = 2;

    [Header("Low Head")]
    [Tooltip("Head/camera to lower when crouched.")]
    public Transform headToLower;
    [HideInInspector] public float? defaultHeadYLocalPosition;
    public float crouchYHeadPosition = 1f;

    [Header("Collider")]
    [Tooltip("Collider to lower when crouched (CapsuleCollider).")]
    public CapsuleCollider colliderToLower;
    [HideInInspector] public float? defaultColliderHeight;

    [Header("Stand Up Check")]
    [Tooltip("Không cho đứng lên nếu vướng trần.")]
    public bool preventStandIfBlocked = true;

    [Tooltip("LayerMask để check vật cản phía trên. Thường để Everything.")]
    public LayerMask obstacleMask = ~0;

    [Tooltip("Nới thêm một chút để check trần chắc chắn.")]
    public float headCheckPadding = 0.05f;

    public bool IsCrouched { get; private set; }
    public event System.Action CrouchStart, CrouchEnd;

    bool wantCrouch; // trạng thái mong muốn khi toggle

    void Reset()
    {
        // Try to get components.
        movement = GetComponentInParent<FirstPersonMovement>();

        // Camera/head
        var cam = movement ? movement.GetComponentInChildren<Camera>() : GetComponentInChildren<Camera>();
        if (cam) headToLower = cam.transform;

        // CapsuleCollider
        colliderToLower = movement ? movement.GetComponentInChildren<CapsuleCollider>() : GetComponentInChildren<CapsuleCollider>();

        // Animator (tìm trong con)
        animator = GetComponentInChildren<Animator>();
    }

    void Awake()
    {
        // cache default
        if (headToLower && !defaultHeadYLocalPosition.HasValue)
            defaultHeadYLocalPosition = headToLower.localPosition.y;

        if (colliderToLower && !defaultColliderHeight.HasValue)
            defaultColliderHeight = colliderToLower.height;

        // init toggle state
        wantCrouch = IsCrouched;
    }

    void Update()
    {
        // INPUT
        if (toggle)
        {
            if (Input.GetKeyDown(key))
                wantCrouch = !wantCrouch;
        }
        else
        {
            wantCrouch = Input.GetKey(key);
        }

        // Nếu muốn đứng lên thì kiểm tra trần trước
        if (!wantCrouch && IsCrouched && preventStandIfBlocked && IsHeadBlocked())
        {
            wantCrouch = true; // ép giữ crouch
        }

        // APPLY STATE
        if (wantCrouch && !IsCrouched) BeginCrouch();
        else if (!wantCrouch && IsCrouched) EndCrouch();
    }

    void LateUpdate()
    {
        // Khi đang crouch: giữ head + collider thấp
        if (IsCrouched)
        {
            LowerHead();
            LowerCollider();
        }
    }

    void BeginCrouch()
    {
        IsCrouched = true;
        SetSpeedOverrideActive(true);

        if (animator) animator.SetBool(crouchBoolName, true);

        CrouchStart?.Invoke();
    }

    void EndCrouch()
    {
        // Rise head
        if (headToLower && defaultHeadYLocalPosition.HasValue)
        {
            headToLower.localPosition = new Vector3(
                headToLower.localPosition.x,
                defaultHeadYLocalPosition.Value,
                headToLower.localPosition.z
            );
        }

        // Reset collider
        if (colliderToLower && defaultColliderHeight.HasValue)
        {
            colliderToLower.height = defaultColliderHeight.Value;
            colliderToLower.center = Vector3.up * colliderToLower.height * 0.5f;
        }

        IsCrouched = false;
        SetSpeedOverrideActive(false);

        if (animator) animator.SetBool(crouchBoolName, false);

        CrouchEnd?.Invoke();
    }

    void LowerHead()
    {
        if (!headToLower) return;

        if (!defaultHeadYLocalPosition.HasValue)
            defaultHeadYLocalPosition = headToLower.localPosition.y;

        headToLower.localPosition = new Vector3(
            headToLower.localPosition.x,
            crouchYHeadPosition,
            headToLower.localPosition.z
        );
    }

    void LowerCollider()
    {
        if (!colliderToLower) return;

        if (!defaultColliderHeight.HasValue)
            defaultColliderHeight = colliderToLower.height;

        float loweringAmount;
        if (defaultHeadYLocalPosition.HasValue)
            loweringAmount = defaultHeadYLocalPosition.Value - crouchYHeadPosition;
        else
            loweringAmount = defaultColliderHeight.Value * 0.5f;

        colliderToLower.height = Mathf.Max(defaultColliderHeight.Value - loweringAmount, 0.2f);
        colliderToLower.center = Vector3.up * colliderToLower.height * 0.5f;
    }

    bool IsHeadBlocked()
    {
        // Không có collider thì thôi
        if (!colliderToLower || !defaultColliderHeight.HasValue) return false;

        // Chiều cao muốn đứng
        float standHeight = defaultColliderHeight.Value;
        float currentHeight = colliderToLower.height;

        float needExtra = (standHeight - currentHeight) + headCheckPadding;
        if (needExtra <= 0f) return false;

        // SphereCast lên trên từ "đỉnh" capsule hiện tại
        float radius = Mathf.Max(0.05f, colliderToLower.radius - 0.01f);

        // world position: transform của collider
        Vector3 worldCenter = colliderToLower.transform.TransformPoint(colliderToLower.center);
        Vector3 top = worldCenter + Vector3.up * (currentHeight * 0.5f - radius);

        return Physics.SphereCast(
            top,
            radius,
            Vector3.up,
            out _,
            needExtra,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        );
    }

    #region Speed override
    void SetSpeedOverrideActive(bool state)
    {
        if (!movement) return;

        if (state)
        {
            if (!movement.speedOverrides.Contains(SpeedOverride))
                movement.speedOverrides.Add(SpeedOverride);
        }
        else
        {
            if (movement.speedOverrides.Contains(SpeedOverride))
                movement.speedOverrides.Remove(SpeedOverride);
        }
    }

    float SpeedOverride() => movementSpeed;
    #endregion
}
