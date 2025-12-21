/**
 * @file GhostHome.cs
 * @brief A script for controlling the initial behavior of a ghost in a Pac-Man-style game, handling its movement from an "inside" position to an "outside" position before transitioning to scattering behavior.
 * * @details This script inherits from GhostBehaviour and is responsible for positioning the ghost at the start of the game. It moves the ghost from the `inside` position (typically within a ghost house) to the `outside` position (just outside the house) after a specified delay. Once the ghost reaches the `outside` position, it transitions to the scattering behavior. The script ensures the ghost's position is valid on the NavMesh and handles cases where the target positions are not directly reachable.
 * * @author [Your Name]
 * @date May 26, 2025
 */

using UnityEngine;
using UnityEngine.AI;

public class GhostHome : GhostBehaviour
{
    public Transform inside;
    public Transform outside;
    [SerializeField] private float exitDelay = 0f;

    private void OnEnable()
    {
        Debug.Log($"{ghost.gameObject.name}: GhostHome OnEnable called. Exit delay: {exitDelay}");
        if (ghost != null && outside != null && inside != null)
        {
            if (!ghost.agent.enabled)
            {
                ghost.agent.enabled = true;
                Debug.Log($"{ghost.gameObject.name}: NavMeshAgent enabled in ExitHome.");
            }
            
            ghost.SetPosition(inside.position, false);
            ghost.isGhostOutFromHome = false;
            Debug.Log($"{ghost.gameObject.name}: Position set to inside: {inside.position}");
            
            Invoke(nameof(ExitHome), exitDelay);
        }
        else
        {
            Debug.LogError($"{ghost.gameObject.name}: Inside, Outside position, or ghost not set in GhostHome! Ghost: {ghost}, Inside: {inside}, Outside: {outside}");
        }
    }

    private void ExitHome()
    {
        Debug.Log($"{ghost.gameObject.name}: ExitHome called.");
        if (ghost != null && outside != null)
        {
            if (!ghost.agent.enabled)
            {
                ghost.agent.enabled = true;
                Debug.Log($"{ghost.gameObject.name}: NavMeshAgent enabled in ExitHome.");
            }

            Vector3 targetPosition = outside.position;
            if (!NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 1f, NavMesh.AllAreas))
            {
                if (NavMesh.SamplePosition(targetPosition, out hit, 10f, NavMesh.AllAreas))
                {
                    targetPosition = hit.position;
                    Debug.Log($"{ghost.gameObject.name}: ExitHome - Snapped outside position to NavMesh: {targetPosition}");
                }
                else
                {
                    Debug.LogError($"{ghost.gameObject.name}: ExitHome - Could not find valid NavMesh near {targetPosition}");
                    return;
                }
            }

            ghost.SetPosition(targetPosition, false);
            ghost.agent.Warp(targetPosition);
            ghost.agent.SetDestination(targetPosition);
            ghost.isGhostOutFromHome = true; // ✅ Only now the ghost starts AI logic
            Debug.Log($"{ghost.gameObject.name}: Moved to outside position: {targetPosition}");

            Invoke(nameof(StartScattering), 0.1f);
        }
        else
        {
            Debug.LogError($"{ghost.gameObject.name}: Outside position or ghost not set in GhostHome.");
        }
    }

    private void StartScattering()
    {
        Debug.Log($"{ghost.gameObject.name}: StartScattering called. Current position: {ghost.transform.position}, Target: {outside.position}");

        float distance = Vector3.Distance(ghost.transform.position, outside.position);

        if (distance <= ghost.agent.stoppingDistance + 0.1f)
        {
            Debug.Log($"{ghost.gameObject.name}: Reached outside position. Disabling GhostHome.");
            ghost.isGhostOutFromHome = true;
            this.Disable(); // ✅ Only disable GhostHome
        }
        else
        {
            NavMeshPath path = new NavMeshPath();
            if (ghost.agent.CalculatePath(outside.position, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                ghost.agent.SetDestination(outside.position);
                Invoke(nameof(StartScattering), 0.1f);
            }
            else if (NavMesh.SamplePosition(outside.position, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                ghost.SetPosition(hit.position, false);
                ghost.agent.Warp(hit.position);
                ghost.agent.SetDestination(hit.position);
                ghost.isGhostOutFromHome = true;
                this.Disable(); // ✅ Only disable GhostHome
            }
            else
            {
                Debug.LogError($"{ghost.gameObject.name}: StartScattering - Could not find valid NavMesh near outside.");
            }
        }
    }


    // Completely remove OnDisable — GhostHome should NOT force enable anything
    private void OnDisable()
    {
        Debug.Log($"{ghost.gameObject.name}: GhostHome OnDisable called. isGhostOutFromHome: {ghost.isGhostOutFromHome}");
    }
}