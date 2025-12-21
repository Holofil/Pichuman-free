using UnityEngine;
using UnityEngine.AI;

public class GhostScatter : GhostBehaviour
{
    private void Update()
    {
        if (enabled && ghost.agent.enabled)
        {
            if (!ghost.agent.pathPending && ghost.agent.remainingDistance <= ghost.agent.stoppingDistance)
            {
                if (!ghost.agent.hasPath || ghost.agent.velocity.sqrMagnitude == 0f)
                {
                    Vector3 destination = ghost.gameManager.GetDestinationPoint();
                    NavMeshPath path = new NavMeshPath();
                    bool hasPath = ghost.agent.CalculatePath(destination, path) && path.status == NavMeshPathStatus.PathComplete;
                    if (hasPath)
                    {
                        ghost.agent.SetDestination(destination);
                        Debug.Log($"{ghost.gameObject.name} Scatter Update - Moving to scatter destination: {destination}, Agent Velocity: {ghost.agent.velocity}");
                    }
                    else
                    {
                        // Fallback: Find the nearest valid NavMesh position to the scatter destination
                        NavMeshHit hit;
                        if (NavMesh.SamplePosition(destination, out hit, 10f, NavMesh.AllAreas))
                        {
                            ghost.agent.SetDestination(hit.position);
                            Debug.Log($"{ghost.gameObject.name} Scatter Update - Cannot find direct path to scatter destination, moving to nearest NavMesh position: {hit.position}");
                        }
                        else
                        {
                            Debug.LogWarning($"{ghost.gameObject.name} Scatter Update - Cannot find a valid NavMesh position near scatter destination: {destination}! Trying a new destination.");
                            destination = ghost.gameManager.GetDestinationPoint();
                        }
                    }
                }
            }
        }
    }

    private void OnDisable()
    {
        if (!ghost.frightened.enabled)
        {
            ghost.chase.Enable();
            Debug.Log($"{ghost.gameObject.name} disabled scatter mode, enabling chase mode.");
        }
    }
}