using UnityEngine;

public class ThirdPersonCameraCollision : MonoBehaviour
{
    [Header("References")]
    public Transform character;     // player root (xoay yaw)
    public Transform pivot;         // điểm đặt ở cổ/đầu (1 empty object con của player)

    [Header("Rotation")]
    public float sensitivity = 2f;
    public float smoothing = 1.5f;

    [Header("Camera Boom")]
    public float distance = 3.0f;       // khoảng cách mong muốn (third person)
    public float minDistance = 0.6f;    // tối thiểu khi bị tường đẩy vào
    public float height = 0.2f;         // nâng camera lên chút so với pivot
    public float sphereRadius = 0.25f;  // “độ dày” chống xuyên

    [Header("Collision")]
    public LayerMask collisionMask = ~0; // set layer tường/địa hình, nhớ loại trừ Player layer

    Vector2 velocity;
    Vector2 frameVelocity;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        // ===== 1) rotate like your FirstPersonLook =====
        Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        Vector2 rawFrameVelocity = Vector2.Scale(mouseDelta, Vector2.one * sensitivity);
        frameVelocity = Vector2.Lerp(frameVelocity, rawFrameVelocity, 1f / smoothing);

        velocity += frameVelocity;
        velocity.y = Mathf.Clamp(velocity.y, -60f, 70f);

        // pitch on pivot, yaw on character
        if (pivot) pivot.localRotation = Quaternion.AngleAxis(-velocity.y, Vector3.right);
        if (character) character.localRotation = Quaternion.AngleAxis(velocity.x, Vector3.up);

        // ===== 2) place camera behind pivot with collision =====
        if (!pivot) return;

        Vector3 pivotPos = pivot.position + Vector3.up * height;
        Vector3 desiredDir = -(pivot.forward); // behind
        float desiredDist = distance;

        // SphereCast from pivot to desired camera position
        float hitDist = desiredDist;
        if (Physics.SphereCast(pivotPos, sphereRadius, desiredDir, out RaycastHit hit, desiredDist, collisionMask, QueryTriggerInteraction.Ignore))
        {
            hitDist = Mathf.Clamp(hit.distance - 0.05f, minDistance, desiredDist);
        }

        Vector3 targetPos = pivotPos + desiredDir * hitDist;

        transform.position = targetPos;
        transform.rotation = Quaternion.LookRotation((pivotPos - transform.position).normalized, Vector3.up);
    }
}
