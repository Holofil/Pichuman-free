using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class TunnelTeleport : MonoBehaviour
{
    [Header("Tags")]
    public string playerTag = "Player";
    public string ghostTag = "Ghost";

    [Header("Target Tunnel")]
    public Transform targetPoint;

    [Header("Offset Distance")]
    public float offsetDistance = 0.5f;

    private bool pacmanOnCooldown = false; // teleport cooldown

    private void OnTriggerEnter(Collider other)
    {
        if (pacmanOnCooldown) return; // prevent infinite loop

        if (other.CompareTag(playerTag))
        {
            StartCoroutine(HandlePacmanTeleport(other));
        }
    }

    IEnumerator HandlePacmanTeleport(Collider pacman)
    {
        pacmanOnCooldown = true; // set cooldown ON

        if (pacman.TryGetComponent<Movement>(out var movement))
        {
            movement.enabled = false;
            Debug.Log($"⛔ Disabled Movement on {pacman.gameObject.name}");

            // Move in direction offset
            Vector3 moveDir = movement.Direction.normalized;
            Vector3 teleportPos = targetPoint.position + moveDir * offsetDistance;

            pacman.transform.position = teleportPos;
            Debug.Log($"🚀 {pacman.gameObject.name} teleported to {teleportPos}");

            // Wait for 2 frames (physics settle)
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            movement.enabled = true;
            Debug.Log($"✅ Re-enabled Movement on {pacman.gameObject.name}");
        }

        // Wait cooldown — 0.5 seconds safe
        yield return new WaitForSeconds(0.5f);

        pacmanOnCooldown = false; // cooldown OFF — can teleport again later
    }
}
