using UnityEngine;

public class GhostFrightened : GhostBehaviour
{
    private float fleeDistance = 10f;
    
    private Material originalMaterial;
    [SerializeField] private Material whiteMaterial;

    public override void Enable(float duration)
    {
        base.Enable(duration);

        // Disable all other behaviors
        ghost.chase.Disable();
        ghost.scatter.Disable();
        ghost.home.Disable();
    }

    public override void Disable()
    {
        base.Disable();
    }

    private void OnEnable()
    {
        Debug.Log($"🔄 {ghost.name} → GhostFrightened.OnEnable called!");

        // Skip if ghost is respawning
        if (ghost.isRespawning)
        {
            Debug.LogWarning($"⚠️ {ghost.name} is respawning, skipping frighten enable!");
            return;
        }

        ghost.agent.speed = ghost.gameManager.GhostSpeed / 2f;

        // Store and swap material
        originalMaterial = ghost.meshRenderer.material;
        if (whiteMaterial != null)
        {
            ghost.meshRenderer.material = whiteMaterial;
        }
        else
        {
            Debug.LogWarning($"⚠️ {ghost.name} has no whiteMaterial assigned.");
        }

        if (ghost.gameManager.isMusicActive)
            ghost.audioSource.mute = true;

        Debug.Log($"{ghost.gameObject.name} enabled frightened mode.");
    }

    private void OnDisable()
    {
        // Skip if ghost is respawning
        if (ghost.isRespawning)
        {
            return;
        }

        ghost.agent.speed = ghost.gameManager.GhostSpeed;

        // Restore original material
        if (originalMaterial != null)
        {
            ghost.meshRenderer.material = originalMaterial;
        }

        int decision = Random.Range(0, 3);
        if (decision == 0)
        {
            ghost.chase.Enable();
            Debug.Log($"{ghost.gameObject.name} disabled frightened mode, enabling chase mode.");
        }
        else
        {
            ghost.scatter.Enable();
            Debug.Log($"{ghost.gameObject.name} disabled frightened mode, enabling scatter mode.");
        }

        if (ghost.gameManager.isMusicActive)
            ghost.audioSource.mute = false;
    }

    private void Update()
    {
        if (!enabled || ghost.isRespawning) return;

        if (ghost.target != null && ghost.agent != null)
        {
            Vector3 awayDirection = (transform.position - ghost.target.position).normalized;
            Vector3 fleeTarget = transform.position + awayDirection * fleeDistance;
            ghost.agent.SetDestination(fleeTarget);
        }
    }
}