using UnityEngine;

public class SimpleTunnelTeleport : MonoBehaviour
{
    public Transform targetPoint;
    public float fixedXOffset = 1.5f;

    private bool hasTeleported = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTeleported) return;

        if (!other.CompareTag("Player")) return;

        if (other.TryGetComponent<Movement>(out var movement))
        {
            Vector3 teleportPos = targetPoint.position;

            if (movement.Direction.x < 0)
            {
                teleportPos.x -= fixedXOffset;
            }
            else if (movement.Direction.x > 0)
            {
                teleportPos.x += fixedXOffset;
            }

            other.transform.position = teleportPos;

            hasTeleported = true;

            Debug.Log($"🚀 Teleported {other.gameObject.name} to {teleportPos}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // when Pacman leaves trigger, allow next teleport
        hasTeleported = false;
    }
}
