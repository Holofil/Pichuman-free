using UnityEngine;
using UnityEngine.AI;

public class GhostChase : GhostBehaviour
{
    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private const float stuckThreshold = 2f;
    private const float stuckDistanceThreshold = 0.1f;

    private void Update()
    {
        if (enabled && ghost.agent.enabled)
        {
            // Check if the ghost is stuck
            float distanceMoved = Vector3.Distance(ghost.transform.position, lastPosition);
            if (distanceMoved < stuckDistanceThreshold)
            {
                stuckTimer += Time.deltaTime;
                if (stuckTimer >= stuckThreshold)
                {
                    Debug.LogWarning($"{ghost.gameObject.name} Chase Update - Ghost appears to be stuck at {ghost.transform.position}! Attempting to reposition.");
                    RepositionToValidNavMesh();
                    stuckTimer = 0f;
                }
            }
            else
            {
                stuckTimer = 0f;
            }
            lastPosition = ghost.transform.position;

            NavMeshPath path = new NavMeshPath();
            bool hasPath = ghost.agent.CalculatePath(ghost.target.position, path) && path.status == NavMeshPathStatus.PathComplete;
            if (hasPath)
            {
                ghost.agent.SetDestination(ghost.target.position);
                Debug.Log($"{ghost.gameObject.name} Chase Update - Moving to Pacman at: {ghost.target.position}, Agent Velocity: {ghost.agent.velocity}");
            }
            else
            {
                // Fallback: Find the nearest valid NavMesh position to Pacman
                NavMeshHit hit;
                if (NavMesh.SamplePosition(ghost.target.position, out hit, 10f, NavMesh.AllAreas))
                {
                    ghost.agent.SetDestination(hit.position);
                    Debug.Log($"{ghost.gameObject.name} Chase Update - Cannot find direct path to Pacman, moving to nearest NavMesh position: {hit.position}");
                }
                else
                {
                    Debug.LogWarning($"{ghost.gameObject.name} Chase Update - Cannot find a valid NavMesh position near Pacman at {ghost.target.position}! Switching to scatter mode.");
                    ghost.scatter.Enable();
                    this.Disable();
                }
            }
        }
    }

    private void RepositionToValidNavMesh()
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(ghost.transform.position, out hit, 10f, NavMesh.AllAreas))
        {
            if (!Physics.CheckSphere(hit.position, 0.5f, LayerMask.GetMask("Obstacle")))
            {
                ghost.transform.position = hit.position;
                ghost.agent.Warp(hit.position);
                Debug.Log($"{ghost.gameObject.name} Chase RepositionToValidNavMesh - Moved to valid NavMesh position: {hit.position}");

                // Reset the destination to Pacman
                if (ghost.target != null)
                {
                    ghost.agent.SetDestination(ghost.target.position);
                }
            }
            else
            {
                Debug.LogWarning($"{ghost.gameObject.name} Chase RepositionToValidNavMesh - Nearest NavMesh position {hit.position} is still inside an obstacle!");
            }
        }
        else
        {
            Debug.LogError($"{ghost.gameObject.name} Chase RepositionToValidNavMesh - Could not find a valid NavMesh position within 10 units of {ghost.transform.position}!");
        }
    }

    private void OnDisable()
    {
        if (!ghost.frightened.enabled)
        {
            ghost.scatter.Enable();
            Debug.Log($"{ghost.gameObject.name} disabled chase mode, enabling scatter mode.");
        }
    }
}