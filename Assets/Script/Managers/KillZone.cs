using UnityEngine;

public class KillZone : MonoBehaviour
{
    [SerializeField] private float minSpeedToKill = 2.0f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Rigidbody rb = other.attachedRigidbody;
        if (!rb) return;

        Vector3 v = rb.linearVelocity;
        v.y = 0f;

        // đi chậm thì không chết
        if (v.magnitude < minSpeedToKill) return;

        // gọi chết
        if (DeathManager.Instance != null)
        {
            DeathManager.Instance.Die(other.gameObject);
        }
    }
}
