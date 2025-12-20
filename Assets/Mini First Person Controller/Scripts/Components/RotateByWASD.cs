using UnityEngine;

public class RotateByWASD : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Usually the camera transform (First Person Camera).")]
    public Transform referenceCamera;

    [Tooltip("The visual model to rotate (ex: Ch31_nonPBR). If null, rotate this transform.")]
    public Transform visualToRotate;

    [Header("Rotation")]
    public float rotateSpeed = 720f; // degrees/second
    public float inputDeadZone = 0.01f;

    void Reset()
    {
        // Auto try find camera
        var cam = GetComponentInChildren<Camera>();
        if (cam) referenceCamera = cam.transform;

        visualToRotate = transform;
    }

    void Update()
    {
        // WASD input
        float x = Input.GetAxisRaw("Horizontal"); // A/D
        float z = Input.GetAxisRaw("Vertical");   // W/S
        Vector3 input = new Vector3(x, 0f, z);

        if (input.sqrMagnitude < inputDeadZone)
            return;

        // Convert input direction to world direction based on camera forward/right
        Vector3 forward = referenceCamera ? referenceCamera.forward : Vector3.forward;
        Vector3 right   = referenceCamera ? referenceCamera.right   : Vector3.right;

        forward.y = 0f; forward.Normalize();
        right.y   = 0f; right.Normalize();

        Vector3 moveDir = (forward * input.z + right * input.x);
        if (moveDir.sqrMagnitude < 0.0001f) return;

        // Rotate visual to face moveDir
        Transform t = visualToRotate ? visualToRotate : transform;
        Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);

        t.rotation = Quaternion.RotateTowards(t.rotation, targetRot, rotateSpeed * Time.deltaTime);
    }
}
