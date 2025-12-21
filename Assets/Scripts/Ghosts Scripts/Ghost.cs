using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Ghost : MonoBehaviour
{
    public GhostHome home { get; private set; }
    public GhostScatter scatter { get; private set; }
    public GhostFrightened frightened { get; private set; }
    public GhostChase chase { get; private set; }
    public NavMeshAgent agent { get; private set; }
    public GameManager gameManager { get; private set; }
    public Vector3 StartingPosition { get; set; }
    public Vector3 Destination { get; set; }
    public AudioSource audioSource { get; private set; }

    public bool isRespawning = false;
    public MeshRenderer meshRenderer;
    public Color ResetColor;
    public GhostBehaviour InitialBehaviour;
    public Transform target;
    public int points = 200;
    [SerializeField] public bool isGhostOutFromHome = false;

    public ParticleSystem DieEffect;
    public float ParticlePauseTime = 1f;

    [SerializeField] AudioClip[] clips;

    [Header("Sine movement")]
    [SerializeField] float Amplitude = 0.1f;
    [SerializeField] float Frequency = 0.05f;
    [SerializeField] public Transform GhostModel;

    [Header("Shooting Variables")]
    [SerializeField] GameObject BulletPrefab;
    [SerializeField] float MinTime, MaxTime;
    [SerializeField] Transform ShootingPoint;
    [SerializeField] float BulletForce = 2f;
    float ShootingTime;

    [Header("Chase Behavior")]
    [SerializeField] private float chaseDistance = 50f;
    
    [Header("Collision Detection")]
    [SerializeField] private float collisionRadius = 1f;
    [SerializeField] private LayerMask pacmanLayer = -1;

    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private const float stuckThreshold = 2f;
    private const float stuckDistanceThreshold = 0.1f;

    private Vector3 lockedPosition;
    private bool positionLocked = false;
    
    // Collision detection variables
    private float collisionCooldown = 0.5f; // Increased cooldown
    private float lastCollisionTime = 0f;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        Debug.Log($"🔍 {name} found GameManager: {(gameManager != null ? gameManager.name : "NULL")}");

        // Ensure components are assigned
        if (home == null) home = GetComponent<GhostHome>();
        if (scatter == null) scatter = GetComponent<GhostScatter>();
        if (frightened == null) frightened = GetComponent<GhostFrightened>();
        if (chase == null) chase = GetComponent<GhostChase>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (meshRenderer != null)
            ResetColor = meshRenderer.material.color;

        isGhostOutFromHome = false;

        // Ensure agent is enabled
        if (agent != null && !agent.enabled)
            agent.enabled = true;

        // Reacquire Pacman
        if (target == null)
        {
            GameObject pacman = GameObject.FindGameObjectWithTag("Player");
            if (pacman != null)
            {
                target = pacman.transform;
            }
            else
            {
                Debug.LogError($"{name} Awake - Could not find Pacman with tag Player!");
            }
        }

        // Optional: auto-enable self
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        // Ensure on NavMesh
        NavMeshHit hit;
        if (!NavMesh.SamplePosition(transform.position, out hit, 10f, NavMesh.AllAreas))
        {
            if (NavMesh.SamplePosition(transform.position, out hit, 50f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                if (agent.enabled)
                    agent.Warp(hit.position);
            }
            else
            {
                Debug.LogError($"{name} Awake - Could not reposition to NavMesh!");
            }
        }
    }


    private void Start()
    {
        StartCoroutine(InitializeGhost());
    }

    private IEnumerator InitializeGhost()
    {
        yield return new WaitForEndOfFrame();

        if (home != null && !home.enabled)
        {
            home.Enable();
            Debug.Log($"🏠 {name} Home enabled.");
        }

        if (scatter != null && !scatter.enabled)
        {
            scatter.Enable();
            Debug.Log($"🌀 {name} Scatter enabled.");
        }

        while (!isGhostOutFromHome)
        {
            yield return null;
        }

        ResetState();
    }


    private void Update()
    {
        // Skip all updates if respawning
        if (isRespawning)
        {
            return;
        }

        if (!isGhostOutFromHome || gameManager.paused)
            return;
        
        // Check for collision with Pacman using distance-based detection
        CheckPacmanCollision();
        
        if (GhostModel != null)
        {
            float hoverOffset = Mathf.Sin(Time.time * Frequency) * Amplitude;
            GhostModel.localPosition = new Vector3(0, hoverOffset, 0);
        }

        if (target == null)
        {
            GameObject pacman = GameObject.FindGameObjectWithTag("Player");
            if (pacman != null)
            {
                target = pacman.transform;
                Debug.Log($"✅ {gameObject.name} reacquired target: {target.name}");
            }
            else
            {
                Debug.LogWarning($"{gameObject.name} Update - Target (Pacman) still null! Waiting...");
                return;
            }
        }

        if (!agent.enabled)
            agent.enabled = true;

        if (agent.hasPath)
        {
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);
            if (distanceMoved < stuckDistanceThreshold)
            {
                stuckTimer += Time.deltaTime;
                if (stuckTimer >= stuckThreshold)
                {
                    RepositionToValidNavMesh();
                    stuckTimer = 0f;
                }
            }
            else
            {
                stuckTimer = 0f;
            }
        }
        lastPosition = transform.position;

        float distanceToPacman = Vector3.Distance(transform.position, target.position);
        NavMeshPath path = new NavMeshPath();
        bool canReachPacman = agent.CalculatePath(target.position, path) && path.status == NavMeshPathStatus.PathComplete;

        if (distanceToPacman <= chaseDistance && !frightened.enabled && canReachPacman)
        {
            if (!chase.enabled)
            {
                scatter.Disable();
                chase.Enable();
            }
        }
        else if (!frightened.enabled)
        {
            if (!scatter.enabled)
            {
                chase.Disable();
                scatter.Enable();
            }
        }

        if (chase.enabled)
        {
            if (canReachPacman)
            {
                agent.SetDestination(target.position);
            }
            else
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(target.position, out hit, 10f, NavMesh.AllAreas))
                    agent.SetDestination(hit.position);
                else
                {
                    scatter.Enable();
                    chase.Disable();
                }
            }
        }
        else if (scatter.enabled)
        {
            if (!agent.pathPending && (agent.remainingDistance <= agent.stoppingDistance || !agent.hasPath))
            {
                Destination = gameManager.GetDestinationPoint();
                bool canReachDestination = agent.CalculatePath(Destination, path) && path.status == NavMeshPathStatus.PathComplete;
                if (canReachDestination)
                    agent.SetDestination(Destination);
                else if (NavMesh.SamplePosition(Destination, out NavMeshHit hit, 10f, NavMesh.AllAreas))
                    agent.SetDestination(hit.position);
            }
        }

        if (!frightened.enabled && gameManager.NumberOfGamesPlayed >= gameManager.GhostShootingGameCount)
            Shoot();

        if (gameManager.isMusicActive && isAudioCompleted)
            SetNewRandomAudioClip();
    }

    // FIXED: Improved collision detection with better safeguards
    private void CheckPacmanCollision()
    {
        if (gameManager == null || target == null || gameManager.paused) return;

        float distanceToPacman = Vector3.Distance(transform.position, target.position);
        float collisionDistance = 1.5f; // Adjust this based on gameplay feel

        if (distanceToPacman <= collisionDistance)
        {
            Debug.Log($"💥 {name} collided with Pacman! Frightened: {frightened.enabled}");

            if (frightened.enabled)
            {
                Debug.Log($"👻 {gameObject.name} is frightened - Ghost gets eaten!");
                gameManager.GhostEaten(this);
                return; // 🛑 prevent further processing like PacmanEaten
            }
            else
            {
                Debug.Log($"💀 {gameObject.name} is NOT frightened - Pacman gets eaten!");
                gameManager.PacmanEaten();
            }
        }
    }


    private void LateUpdate()
    {
        if (positionLocked)
        {
            transform.position = lockedPosition;
        }
    }

    private void RepositionToValidNavMesh()
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 10f, NavMesh.AllAreas))
        {
            if (!Physics.CheckSphere(hit.position, 0.5f, LayerMask.GetMask("Obstacle")))
            {
                transform.position = hit.position;
                agent.Warp(hit.position);

                if (chase.enabled && target != null)
                {
                    agent.SetDestination(target.position);
                }
                else if (scatter.enabled)
                {
                    Destination = gameManager.GetDestinationPoint();
                    agent.SetDestination(Destination);
                }
            }
        }
        else
        {
            Debug.LogError($"{gameObject.name} RepositionToValidNavMesh - Could not find a valid NavMesh position within 10 units of {transform.position}!");
        }
    }

    public bool isAudioCompleted
    {
        get { return !audioSource.loop && (!audioSource.isPlaying && audioSource.time <= 0f); }
    }

    public void ResetState()
    {
        positionLocked = false;
        lastCollisionTime = 0f;
        isRespawning = false; // Reset respawning flag
        
        Amplitude = Random.Range(0.08f, 0.12f);
        Frequency = Random.Range(0.03f, 0.05f);
        meshRenderer.material.color = ResetColor;
        
        // Force re-enable collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
            col.enabled = true;
            Debug.Log($"♻️ Collider re-enabled for {gameObject.name} in ResetState()");
        }
        else
        {
            Debug.LogWarning($"⚠️ Collider missing on {gameObject.name}!");
        }

        // Safe AudioSource handling
        if (audioSource != null)
        {
            if (gameManager.isMusicActive)
            {
                audioSource.enabled = true;
                SetNewRandomAudioClip();
            }
            else
            {
                audioSource.enabled = false;
            }
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: AudioSource component is missing.");
        }
        
        UpdateShootingTime();

        if (!agent.enabled)
        {
            agent.enabled = true;
        }

        frightened.Disable();
        chase.Disable();
        scatter.Enable();

        NavMeshHit hit;
        if (!NavMesh.SamplePosition(transform.position, out hit, 10f, NavMesh.AllAreas))
        {
            if (NavMesh.SamplePosition(transform.position, out hit, 50f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                agent.Warp(hit.position);
            }
            else
            {
                Debug.LogError($"{gameObject.name} ResetState - Could not find a valid NavMesh position within 50 units!");
            }
        }

        Destination = gameManager.GetDestinationPoint();
        if (agent.enabled)
        {
            agent.SetDestination(Destination);
        }

        if (home != null && !home.enabled && !isGhostOutFromHome)
        {
            home.Enable();
        }

        Debug.Log($"{gameObject.name}: ResetState called. isGhostOutFromHome: {isGhostOutFromHome}");
    }

    void SetNewRandomAudioClip()
    {
        audioSource.clip = clips[Random.Range(0, clips.Length)];
        audioSource.Play();
    }

    public void SetPosition(Vector3 position, bool lockPosition = false)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(position, out hit, 1f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
            if (lockPosition)
            {
                lockedPosition = hit.position;
                positionLocked = true;
            }
            else
            {
                positionLocked = false;
            }
            if (agent.enabled)
            {
                agent.Warp(hit.position);
            }
        }
        else
        {
            if (NavMesh.SamplePosition(position, out hit, 10f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                if (lockPosition)
                {
                    lockedPosition = hit.position;
                    positionLocked = true;
                }
                else
                {
                    positionLocked = false;
                }
                if (agent.enabled)
                {
                    agent.Warp(hit.position);
                }
            }
            else
            {
                Debug.LogError($"{gameObject.name} SetPosition - Could not find a valid NavMesh position within 10 units!");
            }
        }
    }

    // REMOVED: OnCollisionEnter and OnTriggerEnter to prevent duplicate collision detection
    // All collision detection is now handled by CheckPacmanCollision() method

    void Shoot()
    {
        if (!frightened.enabled)
        {
            ShootingTime -= Time.deltaTime;
            if (ShootingTime <= 0)
            {
                GameObject bull = Instantiate(BulletPrefab, ShootingPoint.position, ShootingPoint.rotation);
                bull.GetComponent<Rigidbody>().AddForce(ShootingPoint.forward * BulletForce, ForceMode.Impulse);
                UpdateShootingTime();
            }
        }
    }

    void UpdateShootingTime()
    {
        ShootingTime = Random.Range(MinTime, MaxTime);
    }

    ParticleSystem _dieEffect;
    public void GhostDestroy()
    {
        if (gameManager.isGraphicsActive)
        {
            _dieEffect = Instantiate(DieEffect, transform.position, Quaternion.identity);
            _dieEffect.GetComponent<Renderer>().material.color = meshRenderer.material.color;
            Invoke(nameof(DieEffectPaused), ParticlePauseTime);
            Destroy(_dieEffect.gameObject, 5f);
        }

        gameObject.SetActive(false);
    }

    void DieEffectPaused()
    {
        _dieEffect.Pause(true);
    }
}