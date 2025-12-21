using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public class MazePlacements
    {
        public GameObject MazeObject;
        public Vector3[] GhostsPosition;
        public Vector3 PacmanPos;
    }

    [Header("Scene Setup Variables")]
    public MazePlacements[] MazeData;
    public int GhostShootingGameCount = 5;
    public int MazeNumber { get; private set; } = 0;
    public float GhostSpeed = 3f;
    public float MoveingNextLevelTime = 3f;
    public GameObject MoveingNextLevelTextPanel;
    int NumberOfGhosts = 0;
    int mazeLoopPoint = 0, previousSelectedMaze = -1;
    bool GameEnds = false;



    [Header("Game Managing Variables")]
    public int TargetFrameRate = 60;
    public float PacmanReturnTime = 2f;
    public Ghost[] ghosts;
    public Pacman pacman;
    Transform pellets;
    Transform Nodes;
    bool isGameOver = false;
    private float lastPacmanHitTime = -10f;
    [SerializeField] private float pacmanHitCooldown = 1f; // cooldown between hits
    private bool pacmanIsBeingHit = false;
    [SerializeField] private GameObject congratulationsText;
    [SerializeField] private float finalLevelDelay = 3f;   // seconds to wait before returning to menu



    [Header("UI Stuff Variables")]
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI TimeText;
    [Tooltip("Top-center label showing current maze (e.g. 'Maze - 1')")]
    public TextMeshProUGUI mazeLabel;

    public int ghostMultiplier { get; private set; } = 1;
    public int score { get; private set; }
    public int lives { get; private set; }
    public int NumberOfGamesPlayed { get; private set; } = -1;
    public bool paused { get; private set; } = false;
    public AudioManager audioManager { get; private set; }

    [Header("Time Managing Variables")]
    float TotalTimeInSeconds = 0;
    int sec = 0, mint = 0;
    string gameClock = "";

    [Header("Paused Stuff Variables")]
    [SerializeField] GameObject[] CamMovingObjects;
    [SerializeField] GameObject PausedScreen;
    [SerializeField] float ResumeDuration = 3f;
    [SerializeField] GameObject ResumeCountDownObject;

    public bool isMusicActive { get; private set; }
    public bool isGraphicsActive { get; private set; }

    [SerializeField] Transform[] RotatedGameObjects;
    bool timeStop = false;
    
    // Store inside/outside references for ghost respawning
    private Transform currentMazeInside;
    private Transform currentMazeOutside;
    
    // Flag to prevent multiple initialization calls
    private bool isInitialized = false;

    private void Awake()
    {
        Debug.Log($"📍 GameManager loaded in scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        // ✅ Step 0: Reset initialization in case of returning to scene
        isInitialized = false;

        // ✅ Step 1: Prevent duplicate GameManagers
        GameManager[] managers = FindObjectsOfType<GameManager>();
        if (managers.Length > 1 && managers[0] != this)
        {
            Debug.Log("🗑️ Destroying duplicate GameManager");
            Destroy(gameObject);
            return;
        }

        Debug.Log("🎮 GameManager AWAKE() called!");

        // ✅ Step 3: Ensure we don't initialize multiple times
        if (isInitialized)
        {
            Debug.LogWarning("GameManager already initialized, skipping Awake()");
            return;
        }

        try
        {
            Application.targetFrameRate = TargetFrameRate;

            // 🔁 Load maze settings
            MazeNumber = PlayerPrefs.GetInt("MazeNumber", 0);
            Debug.Log($"🔄 Loading MazeNumber: {MazeNumber} (Maze{MazeNumber + 1})");

            // ⚠️ Validate MazeData
            if (MazeData == null || MazeData.Length == 0)
            {
                Debug.LogError("🚨 MazeData array is null or empty! Assign maze data in the Inspector.");
                return;
            }

            if (MazeNumber < 0 || MazeNumber >= MazeData.Length)
            {
                Debug.LogWarning($"⚠️ MazeNumber {MazeNumber} is outside MazeData bounds. Defaulting to 0.");
                MazeNumber = 0;
                PlayerPrefs.SetInt("MazeNumber", 0);
                PlayerPrefs.Save();
            }

            // 👻 Load ghost settings
            NumberOfGhosts = PlayerPrefs.GetInt("NumberOfGhosts", 3);
            int ghostSpeedVal = PlayerPrefs.GetInt("GhostsSpeed", 1);
            switch (ghostSpeedVal)
            {
                case 1: GhostSpeed = 3f; break;
                case 2: GhostSpeed = 5f; break;
                case 3: GhostSpeed = 6f; break;
                default: GhostSpeed = 3f; break;
            }

            Debug.Log($"👻 Number of Ghosts: {NumberOfGhosts}");
            Debug.Log($"⚡ Ghost Speed: {GhostSpeed}");

            // 🧩 Initialize everything
            SettingSetup();
            SetupInitialMaze();
            InitializeTimeManagingVariables();

            // 🎮 Game state defaults
            Time.timeScale = 1f;
            mazeLoopPoint = MazeNumber;
            GameEnds = false;
            previousSelectedMaze = -1;

            // ✅ Mark initialized
            isInitialized = true;
            Debug.Log("✅ GameManager Awake complete.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"🚨 Error in GameManager Awake: {e.Message}\n{e.StackTrace}");
            isInitialized = false;
        }
    }

    private void SetupInitialMaze()
    {
        // Deactivate all maze prefabs first
        for (int i = 0; i < MazeData.Length; i++)
        {
            if (MazeData[i].MazeObject != null)
            {
                MazeData[i].MazeObject.SetActive(false);
            }
        }

        // Find maze by tag
        string mazeTag = $"Maze{MazeNumber + 1}";
        GameObject foundMaze = GameObject.FindGameObjectWithTag(mazeTag);

        if (foundMaze != null)
        {
            Debug.Log($"✅ Maze {mazeTag} found: {foundMaze.name}");
            MazeData[MazeNumber].MazeObject = foundMaze;
            MazeData[MazeNumber].MazeObject.SetActive(true);
        }
        else
        {
            Debug.LogError($"🚧 Maze prefab for {mazeTag} not found!");
            // Try to use the assigned maze object instead
            if (MazeData[MazeNumber].MazeObject != null)
            {
                MazeData[MazeNumber].MazeObject.SetActive(true);
                Debug.Log($"Using assigned maze object: {MazeData[MazeNumber].MazeObject.name}");
            }
        }

        // Debug all MazeData slots
        for (int i = 0; i < MazeData.Length; i++)
        {
            var data = MazeData[i];
            if (data.MazeObject != null)
            {
                Debug.Log($"ℹ️ Maze {i} - Active: {data.MazeObject.activeSelf}, Name: {data.MazeObject.name}");
            }
        }

        // Update the top-center UI label for the current maze
        UpdateMazeLabel();
    }

    private void OnDisable()
    {
        isInitialized = false;
        Debug.LogError("🚫 GameManager got DISABLED! Stack trace:\n" + System.Environment.StackTrace);
    }

    IEnumerator FinalPhysicsFix()
    {
        yield return new WaitForSeconds(0.5f); // wait for everything to spawn

        GameObject pacman = GameObject.FindGameObjectWithTag("Player");
        if (pacman == null)
        {
            Debug.LogError("❌ PACMAN NOT FOUND in FinalPhysicsFix!");
            yield break;
        }

        Collider pacmanCollider = pacman.GetComponent<Collider>();
        if (pacmanCollider == null)
        {
            Debug.LogError("❌ PACMAN HAS NO COLLIDER!");
            yield break;
        }

        Ghost[] ghostList = FindObjectsOfType<Ghost>();
        Debug.Log($"🔄 Fixing collision between Pacman and {ghostList.Length} ghosts.");

        foreach (Ghost ghost in ghostList)
        {
            if (ghost == null) continue;

            Collider ghostCol = ghost.GetComponent<Collider>();
            if (ghostCol != null)
            {
                Physics.IgnoreCollision(pacmanCollider, ghostCol, false);
                Debug.Log($"✅ Enabled collision: Pacman ↔ {ghost.name}");
            }
            else
            {
                Debug.LogWarning($"⚠️ Ghost {ghost.name} has no collider!");
            }
        }

        Physics.SyncTransforms();
        Debug.Log("✅ FinalPhysicsFix completed. All collisions re-enabled.");
    }

    private void Start()
    {
        // 🔄 Force-enable Layer Collision Matrix
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Ghosts"), false);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Ghosts"), LayerMask.NameToLayer("Player"), false);

        Debug.Log("✅ Collision matrix between Player <-> Ghosts re-enabled manually.");

        StartCoroutine(FixPhysicsLayers());
        RefreshGhostsList();
        Debug.Log("🎮 GameManager START() called!");

        // --- ADD THIS BLOCK HERE ---
        GameObject pacman = GameObject.FindGameObjectWithTag("Player");
        if (pacman != null)
        {
            Debug.Log($"✅ PACMAN FOUND: {pacman.name}, Layer: {LayerMask.LayerToName(pacman.layer)}, Active: {pacman.activeInHierarchy}");

            Rigidbody rb = pacman.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Debug.Log($"✅ PACMAN RIGIDBODY: isKinematic = {rb.isKinematic}, useGravity = {rb.useGravity}");
            }
            else
            {
                Debug.LogError("❌ PACMAN HAS NO RIGIDBODY!!!");
            }

            Collider[] colliders = pacman.GetComponents<Collider>();
            foreach (var c in colliders)
            {
                Debug.Log($"✅ PACMAN COLLIDER: {c.GetType().Name}, Enabled: {c.enabled}, isTrigger: {c.isTrigger}");
            }
        }
        else
        {
            Debug.LogError("❌ PACMAN NOT FOUND!!!");
        }
        // --- END OF BLOCK ---

        if (!isInitialized)
        {
            Debug.LogError("GameManager was not properly initialized in Awake()!");
            return;
        }

        try
        {
            audioManager = FindObjectOfType<AudioManager>();
            if (audioManager == null)
            {
                Debug.LogError("AudioManager not found in the scene!");
            }

            // NEW: wait for one frame
            StartCoroutine(DelayedStart());
            StartCoroutine(FinalPhysicsFix());

        }
        catch (System.Exception e)
        {
            Debug.LogError($"🚨 Error in GameManager Start: {e.Message}\n{e.StackTrace}");
        }
    }

    IEnumerator FixPhysicsLayers()
    {
        yield return new WaitForSeconds(0.1f); // wait 1 frame after scene load
        Physics.SyncTransforms();
        Debug.Log("✅ Physics SyncTransforms done!");
    }

    private IEnumerator DelayedStart()
    {
        yield return new WaitForEndOfFrame(); // Wait one frame

        SetupCurrentScene();
        StartNewGame();
    }

    public void RefreshGhostsList()
    {
        ghosts = FindObjectsOfType<Ghost>();
        Debug.Log($"🔄 GameManager updated ghosts list: {ghosts.Length} ghosts found.");
    }

    void SettingSetup()
    {
        isMusicActive = PlayerPrefs.GetString("MusicStatus", "ON") == "ON";
        isGraphicsActive = PlayerPrefs.GetString("GraphicsStatus", "ON") == "ON";
    }

    private void Update()
    {
        if (!isInitialized) return;

        try
        {
            // Main game update logic
            if (!paused && !isGameOver && !timeStop)
            {
                UpdateTime(Time.deltaTime);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"🚨 Error in GameManager Update: {e.Message}\n{e.StackTrace}");
        }
    }

    void UpdateTime(float deltaTime)
    {
        TotalTimeInSeconds += deltaTime;
        
        int totalSeconds = Mathf.FloorToInt(TotalTimeInSeconds);
        mint = totalSeconds / 60;
        sec = totalSeconds % 60;

        gameClock = "Time " + TimeMaking();

        if (TimeText != null)
        {
            TimeText.text = gameClock;
        }
        else
        {
            Debug.LogError("🚨 TimeText is NULL!");
        }
    }

    string TimeMaking()
    {
        return $"{(mint <= 9 ? "0" + mint : mint.ToString())} : {(sec <= 9 ? "0" + sec : sec.ToString())}";
    }

    void SetSceneLoop()
    {
        Debug.Log("🔁 SetSceneLoop called!");

        if (previousSelectedMaze != -1)
        {
            if (pellets != null)
                pellets.gameObject.SetActive(false);

            if (MazeData != null && previousSelectedMaze < MazeData.Length && MazeData[previousSelectedMaze].MazeObject != null)
                MazeData[previousSelectedMaze].MazeObject.SetActive(false);

            if (isMusicActive && audioManager != null)
                audioManager.Play("winning");
        }

        int numberOfGames = PlayerPrefs.GetInt("NumberOfGames", 0) + 1;
        PlayerPrefs.SetInt("NumberOfGames", numberOfGames);

        // ✅ First increment the maze
        previousSelectedMaze = MazeNumber;
        MazeNumber = (MazeNumber + 1) % MazeData.Length;

        if (MazeNumber == mazeLoopPoint)
        {
            if (GhostSpeed == 6f)
            {
                GameEnds = true;
            }
            else if (GhostSpeed < 6f)
            {
                GhostSpeed += 0.5f;
            }
        }

        // ✅ Now initialize the next maze
        InitializeTimeManagingVariables();
        SetupCurrentScene();
        StartNewGame();
    }


    void InitializeTimeManagingVariables()
    {
        TotalTimeInSeconds = 0;
        sec = 0;
        mint = 0;
        gameClock = "";
        timeStop = false;   
        isGameOver = false;
    }

    void SetupCurrentScene()
    {
        Physics.SyncTransforms();
        Debug.Log("🔁 Physics.SyncTransforms() in SetupCurrentScene");

        try
        {
            if (MazeData == null || MazeNumber >= MazeData.Length || MazeData[MazeNumber].MazeObject == null)
            {
                Debug.LogError($"❌ Error: MazeData[{MazeNumber}] is null or out of bounds!");
                return;
            }

            GameObject Maze = MazeData[MazeNumber].MazeObject;
            Maze.SetActive(true);
            Debug.Log($"✅ Maze {MazeNumber} activated: {Maze.name}");

            // Update maze label whenever a maze is activated
            UpdateMazeLabel();

            // ✅ Handle NavMeshSurface per maze
            foreach (NavMeshSurface surface in FindObjectsOfType<NavMeshSurface>())
                surface.enabled = false;

            NavMeshSurface mazeSurface = Maze.GetComponent<NavMeshSurface>();
            if (mazeSurface != null)
            {
                mazeSurface.enabled = true;
                Debug.Log("✅ Activated NavMeshSurface for current maze.");
            }
            else
            {
                Debug.LogWarning("⚠️ No NavMeshSurface found on the active maze.");
            }

           
            // ✅ Assign ghosts
            Transform ghostContainer = Maze.transform.Find("Ghosts");
            if (ghostContainer == null)
            {
                Debug.LogError("❌ Error: 'Ghosts' object not found in the Maze");
                return;
            }

            List<Ghost> ghostList = new List<Ghost>();
            foreach (Transform child in ghostContainer)
            {
                Ghost ghost = child.GetComponent<Ghost>();
                if (ghost != null)
                {
                    ghostList.Add(ghost);
                    Debug.Log($"✅ Found ghost in maze: {ghost.name}");
                }
            }

            ghosts = ghostList.ToArray();
            Debug.Log($"👻 Total ghosts assigned: {ghosts.Length}");

            // Setup pellets
            // FIXED: Always search for "Pellets"
            Transform pelletTransform = Maze.transform.Find("Pellets");
            if (pelletTransform == null)
            {
                Debug.LogError($"❌ Error: Pellets object not found in Maze {MazeNumber}");
                return;
            }

            pellets = pelletTransform;
            pellets.gameObject.SetActive(true);
            Debug.Log($"✅ Pellets enabled for Maze {MazeNumber}");

            // Setup nodes
            Nodes = Maze.transform.Find("Nodes");
            if (Nodes == null)
            {
                Debug.LogError($"❌ Error: Nodes object not found in Maze {MazeNumber}");
                return;
            }
            Debug.Log($"Nodes found with {Nodes.childCount} children");

            // Inside/outside setup
            currentMazeInside = FindInsideTransform(Maze);
            currentMazeOutside = FindOutsideTransform(Maze);

            if (currentMazeInside == null || currentMazeOutside == null)
            {
                Debug.LogError("❌ Could not find inside/outside transforms");
                return;
            }

            // Setup ghosts
            SetupGhosts(currentMazeInside, currentMazeOutside);

            // Setup Pacman
            if (pacman != null)
            {
                if (MazeData[MazeNumber].PacmanPos != Vector3.zero)
                {
                    pacman.transform.position = MazeData[MazeNumber].PacmanPos;
                }
                Debug.Log($"Pacman positioned at: {pacman.transform.position}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"🚨 Error in SetupCurrentScene: {e.Message}\n{e.StackTrace}");
        }
    }


    Transform FindInsideTransform(GameObject maze)
    {
        Transform inside = maze.transform.Find("inside and outside/inside");
        if (inside == null)
            inside = maze.transform.Find("Inside and Outside/Inside");
        if (inside == null)
            inside = maze.transform.Find("inside and outside/Inside");
        
        if (inside != null)
            Debug.Log($"Inside Transform found at: {inside.position}");
        else
            Debug.LogError("Inside transform not found!");
            
        return inside;
    }

    Transform FindOutsideTransform(GameObject maze)
    {
        Transform outside = maze.transform.Find("inside and outside/outside");
        if (outside == null)
            outside = maze.transform.Find("Inside and Outside/Outside");
        if (outside == null)
            outside = maze.transform.Find("inside and outside/Outside");
        
        if (outside != null)
            Debug.Log($"Outside Transform found at: {outside.position}");
        else
            Debug.LogError("Outside transform not found!");
            
        return outside;
    }

    void SetupGhosts(Transform inside, Transform outside)
    {
        Debug.Log($"👻 Setting up {NumberOfGhosts} out of {ghosts.Length} available ghosts");
        
        // Disable all ghosts first
        for (int i = 0; i < ghosts.Length; i++)
        {
            if (ghosts[i] != null)
            {
                ghosts[i].gameObject.SetActive(false);
            }
        }
        
        // Activate only the selected number of ghosts
        for (int i = 0; i < NumberOfGhosts && i < ghosts.Length; i++)
        {
            if (ghosts[i] == null)
            {
                Debug.LogError($"⚠️ Ghost at index {i} is null!");
                continue;
            }

            ghosts[i].gameObject.SetActive(true);
            ghosts[i].SetPosition(inside.position);
            ghosts[i].agent.speed = GhostSpeed;
            
            // Setup ghost home
            ghosts[i].home.inside = inside;
            ghosts[i].home.outside = outside;
            ghosts[i].isGhostOutFromHome = false;
            ghosts[i].home.Enable();
            
            Debug.Log($"👻 Ghost {i} ({ghosts[i].name}) setup complete");
        }
    }

    void SaveData(int _mazeNumber, int _numberOfGhosts, float _ghostSpeed)
    {
        int ghostSpeedType = 0;
        if (_ghostSpeed == 3f)
            ghostSpeedType = 1;
        else if (_ghostSpeed == 5f)
            ghostSpeedType = 2;
        else if (_ghostSpeed == 6f)
            ghostSpeedType = 3;

        string SaveTimeStr = _mazeNumber.ToString() + _numberOfGhosts.ToString() + ghostSpeedType.ToString();
        string KeyTotalTimeInSec = SaveTimeStr + "TotalTime";
        string KeyTotalTimeInStr = SaveTimeStr + "TotalTimeStr";

        int previousTime = PlayerPrefs.GetInt(KeyTotalTimeInSec, int.MaxValue);
        if (TotalTimeInSeconds < previousTime)
        {
            PlayerPrefs.SetInt(KeyTotalTimeInSec, Mathf.RoundToInt(TotalTimeInSeconds));
            PlayerPrefs.SetString(KeyTotalTimeInStr, TimeMaking());
        }
    }

    private void StartNewGame()
    {
        Debug.Log("🎮 StartNewGame() called!");

        try
        {
            SetScore(0);

            // Set lives based on ghost settings
            if (GhostSpeed == 3f)
            {
                if (NumberOfGhosts == 4)
                    SetLives(5);
                else if (NumberOfGhosts == 5)
                    SetLives(6);
                else
                    SetLives(4);
            }
            else if (GhostSpeed == 5f)
            {
                SetLives(6);
            }
            else if (GhostSpeed == 6f)
            {
                SetLives(8);
            }

            Debug.Log($"❤️ Lives set to {lives} based on Ghosts: {NumberOfGhosts}, Speed: {GhostSpeed}");

            StartNewRound();
            // --- ADD THIS BLOCK ---
            for (int i = NumberOfGhosts; i < ghosts.Length; i++)
            {
                if (ghosts[i] != null)
                    ghosts[i].gameObject.SetActive(false);
            }
        // -----------------------
        }
        catch (System.Exception e)
        {
            Debug.LogError($"🚨 Error in StartNewGame: {e.Message}\n{e.StackTrace}");
        }
    }

    // Add this new method for initial game start
    private void InitialGameSetup()
    {
        try
        {
            // Reset active ghosts
            for (int i = 0; i < NumberOfGhosts && i < ghosts.Length; i++)
            {
                if (ghosts[i] != null && ghosts[i].gameObject.activeSelf)
                {
                    ghosts[i].ResetState();
                }
            }

            // Reset pacman to spawn position
            if (pacman != null)
            {
                pacman.ResetState();
                Debug.Log($"Pacman reset to position: {pacman.transform.position}");
            }
            else
            {
                Debug.LogError("❌ Pacman reference is null!");
            }

            // Start game immediately without countdown
            paused = false;
            Time.timeScale = 1f;
            
            foreach (var item in CamMovingObjects)
                item.SetActive(true);
            
            if (pacman != null)
                pacman.gameObject.SetActive(true);
                
            PausedScreen.SetActive(false);
            ResumeCountDownObject.SetActive(false);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"🚨 Error in InitialGameSetup: {e.Message}\n{e.StackTrace}");
        }
    }

    // Keep ResetState for death scenarios only
    private void ResetState()
    {
        try
        {
            // Reset active ghosts
            for (int i = 0; i < NumberOfGhosts && i < ghosts.Length; i++)
            {
                if (ghosts[i] != null && ghosts[i].gameObject.activeSelf)
                {
                    ghosts[i].ResetState();
                }
            }

            // Reset pacman to spawn position
            if (pacman != null)
            {
                pacman.ResetState();
                Debug.Log($"Pacman reset to position: {pacman.transform.position}");
            }
            else
            {
                Debug.LogError("❌ Pacman reference is null!");
            }

            // Always show countdown for death resets
            paused = true;
            ResetResume();  // This starts the resume countdown
        }
        catch (System.Exception e)
        {
            Debug.LogError($"🚨 Error in ResetState: {e.Message}\n{e.StackTrace}");
        }
    }

    // Then in StartNewRound(), replace ResetState() with InitialGameSetup()
    private void StartNewRound()
    {
        try
        {
            NumberOfGamesPlayed++;
            gameOverText.enabled = false;

            // ✅ Reactivate all pellets (including nested)
            if (pellets != null)
            {
                int pelletCount = 0;
                foreach (Transform child in pellets.GetComponentsInChildren<Transform>(true))
                {
                    if (child.CompareTag("Pellet"))
                    {
                        child.gameObject.SetActive(true);
                        pelletCount++;
                    }
                }
                Debug.Log($"✅ Reactivated {pelletCount} pellets for new round.");
            }
            else
            {
                Debug.LogError("❌ pellets is NULL in StartNewRound!");
            }

            // 🔧 Use InitialGameSetup instead of ResetState for new games
            InitialGameSetup();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"🚨 Error in StartNewRound: {e.Message}\n{e.StackTrace}");
        }
    }



    private void GameOver()
    {
        Debug.Log("💀 Game Over triggered");
        
        // Show Game Over text
        gameOverText.enabled = true;

        if (isMusicActive && audioManager != null)
            audioManager.PlayNewSound("gameover");

        isGameOver = true;

        // Stop all ghost movement & destroy active ones
        for (int i = 0; i < NumberOfGhosts && i < ghosts.Length; i++)
        {
            if (ghosts[i] != null && ghosts[i].gameObject.activeSelf)
            {
                ghosts[i].agent.isStopped = true;      // Stop movement
                ghosts[i].GhostDestroy();              // Optional: disable visuals/sound
            }
        }

        // Optional: stop Pacman movement as well
        if (pacman != null)
        {
            pacman.gameObject.SetActive(false);
        }
        
        // Immediately go to UI Scene (or wait for 1 second for sound/text)
        StartCoroutine(WaitForReturningToUIScene(1f)); // You can set it to 0f for instant transition
    }

    IEnumerator WaitForReturningToUIScene(float remainingTime)
    {
        yield return new WaitForSeconds(remainingTime);
        SceneManager.LoadScene("ui scene");
    }


    public Vector3 GetDestinationPoint()
    {
        if (Nodes == null || Nodes.childCount == 0)
        {
            Debug.LogWarning("Nodes is null or has no children! Using fallback.");
            Vector3 randomPoint = new Vector3(Random.Range(-10f, 10f), 0f, Random.Range(-10f, 10f));
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 10f, NavMesh.AllAreas))
            {
                return hit.position;
            }
            return Vector3.zero;
        }

        Vector3 destination = Nodes.GetChild(Random.Range(0, Nodes.childCount)).position;
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(destination, out navHit, 10f, NavMesh.AllAreas))
        {
            return navHit.position;
        }

        return GetDestinationPoint();
    }

    private void SetLives(int newLives)
    {
        lives = Mathf.Max(0, newLives);

        if (livesText != null)
        {
            livesText.text = lives.ToString();
        }
        else
        {
            Debug.LogError("❌ livesText is NULL!");
        }
    }

    private void SetScore(int newScore)
    {
        score = newScore;
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString().PadLeft(2, '0');
        }
    }
    
    // Update the top-center maze label (shows 1-based maze index)
    private void UpdateMazeLabel()
    {
        if (mazeLabel != null)
        {
            int displayIndex = Mathf.Clamp(MazeNumber + 1, 1, MazeData != null ? MazeData.Length : MazeNumber + 1);
            mazeLabel.text = $"Maze - {displayIndex}";
        }
        else
        {
            // Optional: uncomment to debug missing assignment
            // Debug.LogWarning("Maze label (mazeLabel) is not assigned in the Inspector.");
        }
    }
    private void ResetPacmanAfterHit()
    {
        if (pacman != null)
        {
            pacman.ResetState();
        }
        pacmanIsBeingHit = false;
    }

    public void PacmanEaten()
    {
        if (pacmanIsBeingHit) return;

        pacmanIsBeingHit = true;
        int updatedLives = lives - 1;
        SetLives(updatedLives);

        if (isMusicActive && audioManager != null)
            audioManager.Play("die");

        if (lives > 0)
        {
            // ✅ Trigger Pacman’s death animation
            if (pacman != null)
                pacman.DeathSequence();

            StartCoroutine(ResetAfterDeathDelay(2f));
        }
        else
        {
            if (pacman != null)
                pacman.DeathSequence();

            GameOver();
        }
    }

    private IEnumerator ResetAfterDeathDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // ✅ Use scaled time
        ResetState(); // ⏮ Reset pacman and ghosts
        pacmanIsBeingHit = false;
    }



    // FIXED: Completely rewritten ghost respawn system
    // GameManager.cs

// (Rest of the code remains the same)

    // GameManager.cs

// (Rest of the code remains the same)

    public void GhostEaten(Ghost ghost)
    {
        Debug.Log($"👻 {ghost.name} eaten! Triggering respawn sequence...");

        // Award points
        SetScore(score + (ghost.points * ghostMultiplier));
        if (isMusicActive && audioManager != null)
        {
            audioManager.PlayNewSound("ghost die");
        }

        // Call the respawn coroutine to handle the entire respawn process
        StartCoroutine(RespawnGhost(ghost));

        // Reset the ghost multiplier.
        ghostMultiplier = 1;
    }

    private IEnumerator RespawnGhost(Ghost ghost)
    {
        if (ghost == null || ghost.agent == null)
        {
            Debug.LogError($"⚠️ Ghost or NavMeshAgent is null during respawn!");
            yield break;
        }

        Debug.Log($"🔄 Starting respawn sequence for {ghost.name}");

        // Step 1: Immediately disable all ghost behaviors and collision
        ghost.frightened.Disable();
        ghost.chase.Disable();
        ghost.scatter.Disable();

        // Disable collider to prevent further collisions
        Collider ghostCollider = ghost.GetComponent<Collider>();
        if (ghostCollider != null)
        {
            ghostCollider.enabled = false;
            Debug.Log($"🚫 Disabled collider for {ghost.name}");
        }

        // Stop NavMeshAgent
        if (ghost.agent.enabled)
        {
            ghost.agent.ResetPath();
            ghost.agent.velocity = Vector3.zero;
            ghost.agent.isStopped = true;
        }

        // Step 2: Make ghost invisible
        if (ghost.GhostModel != null)
        {
            ghost.GhostModel.gameObject.SetActive(false);
            Debug.Log($"👻 Made {ghost.name} invisible.");
        }
        
        // Step 3: Teleport to inside position
        if (currentMazeInside != null)
        {
            Vector3 respawnPos = currentMazeInside.position;
            ghost.transform.position = respawnPos;
            
            if (ghost.agent.enabled)
            {
                ghost.agent.Warp(respawnPos);
                ghost.agent.nextPosition = respawnPos;
            }
            Debug.Log($"📍 Teleported {ghost.name} to inside position: {respawnPos}");
        }

        // Step 4: Wait for respawn delay
        float respawnDelay = 3f;
        Debug.Log($"⏳ Waiting {respawnDelay} seconds for {ghost.name} respawn...");
        yield return new WaitForSeconds(respawnDelay);

        // Step 5: Re-enable ghost
        if (ghost != null && ghost.gameObject != null)
        {
            // Restore visibility
            if (ghost.GhostModel != null)
            {
                ghost.GhostModel.gameObject.SetActive(true);
                Debug.Log($"✅ Restored visibility for {ghost.name}");
            }

            // Re-enable collider
            if (ghostCollider != null)
            {
                ghostCollider.enabled = true;
                Debug.Log($"✅ Re-enabled collider for {ghost.name}");
            }

            // Start agent again
            if (ghost.agent.enabled)
            {
                ghost.agent.isStopped = false;
            }

            // Reset the ghost's color and state to its original, non-frightened state
            ghost.ResetState();

            // Enable home behavior to move ghost out
            if (ghost.home != null)
            {
                ghost.home.Enable();
                Debug.Log($"🏠 Enabled home behavior for {ghost.name}");
            }

            Debug.Log($"✅ {ghost.name} respawn complete!");
        }
        else
        {
            Debug.LogError($"❌ Ghost {ghost.name} was destroyed during respawn!");
        }
    }

    public void PelletEaten(Pellet pellet, bool isPowerPellet = false)
    {
        pellet.gameObject.SetActive(false);
        SetScore(score + pellet.points);

        if (isMusicActive && audioManager != null)
        {
            audioManager.Play(isPowerPellet ? "powerpellet" : "pellet");
        }

        Debug.Log("🍽 Pellet consumed: " + pellet.name);

        // ✅ Only advance when the CURRENT maze truly has zero pellets left.
        if (!HasRemainingPellets())
        {
            Debug.Log($"🎉 All pellets cleared in Maze {MazeNumber}, triggering level complete!");

            // Hide Pacman and stop game logic
            if (pacman != null)
                pacman.gameObject.SetActive(false);
            SaveData(MazeNumber, NumberOfGhosts, GhostSpeed);

            timeStop = true;
            isGameOver = true;

            // Always show congratulations and next level panel
            if (MoveingNextLevelTextPanel != null)
                MoveingNextLevelTextPanel.SetActive(true);
            if (congratulationsText != null)
                congratulationsText.SetActive(true);

            // If this is the last maze, stop all movement and timer, then start coroutine to end game and go to UI scene
            if (MazeNumber == MazeData.Length - 1)
            {
                Debug.Log("🏁 Last maze completed! Stopping all movement and timer, then returning to UI scene after congratulations screen.");
                // Stop all ghost movement
                for (int i = 0; i < ghosts.Length; i++)
                {
                    if (ghosts[i] != null)
                    {
                        ghosts[i].agent.isStopped = true;
                        ghosts[i].gameObject.SetActive(false);
                    }
                }
                // Stop timer and all movement
                Time.timeScale = 0f;
                // Prevent Update from running
                isGameOver = true;
                timeStop = true;
                StartCoroutine(EndGameAfterLastMaze(MoveingNextLevelTime));
                return;
            }

            // Only call GameStartsAgain if NOT on last maze
            if (GameEnds)
                GameStartsAgain();

            // Otherwise, advance to the next maze
            Invoke(nameof(AdvanceToNextMaze), MoveingNextLevelTime);
        }
    // Coroutine to end game after last maze
    IEnumerator EndGameAfterLastMaze(float delay)
    {
        // Wait using unscaled time since Time.timeScale = 0
        float timer = 0f;
        while (timer < delay)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        SceneManager.LoadScene("ui scene");
    }
    }


    private void AdvanceToNextMaze()
    {
        // deactivate old maze
        MazeData[MazeNumber].MazeObject.SetActive(false);

        MazeNumber++;
        // Ensure MazeNumber does not exceed bounds
        if (MazeNumber >= MazeData.Length)
        {
            MazeNumber = MazeData.Length - 1;
            Debug.LogWarning($"MazeNumber exceeded bounds, clamped to {MazeNumber}");
        }
        // Update the label to reflect the new maze (1-based display)
        UpdateMazeLabel();
        PlayerPrefs.SetInt("MazeNumber", MazeNumber);
        PlayerPrefs.Save();

        // Activate the correct maze object
        if (MazeData[MazeNumber].MazeObject != null)
        {
            MazeData[MazeNumber].MazeObject.SetActive(true);
            Debug.Log($"✅ Maze {MazeNumber} activated: {MazeData[MazeNumber].MazeObject.name}");
        }
        else
        {
            Debug.LogError($"❌ MazeObject for Maze {MazeNumber} is null!");
        }

        // Update pellets reference to the new maze's pellet container
        // Original line to change in AdvanceToNextMaze()
        // FIXED: Always search for "Pellets"
        Transform pelletTransform = MazeData[MazeNumber].MazeObject != null ? MazeData[MazeNumber].MazeObject.transform.Find("Pellets") : null;
        if (pelletTransform != null)
        {
            pellets = pelletTransform;
            pellets.gameObject.SetActive(true);
            Debug.Log($"✅ Pellets reference updated for Maze {MazeNumber}");
        }
        else
        {
            pellets = null;
            Debug.LogError($"❌ Pellets object not found in Maze {MazeNumber} during AdvanceToNextMaze");
        }

        // Reset scoreboard and timer for new maze
        SetScore(0);
        InitializeTimeManagingVariables();

        if (pacman != null)
        {
            pacman.gameObject.SetActive(true);
            pacman.ResetState();
        }

        if (MoveingNextLevelTextPanel != null) MoveingNextLevelTextPanel.SetActive(false);
        if (congratulationsText != null)       congratulationsText.SetActive(false);

        // Ensure full initialization for the new maze
        SetupCurrentScene();
        StartNewGame();
    }


    // public void PelletEaten(Pellet pellet, bool isPowerPellet = false)
    // {
    //     if (pellet == null)
    //     {
    //         Debug.LogError("❌ Null pellet passed to PelletEaten!");
    //         return;
    //     }

    //     Debug.Log($"🍽️ Pellet eaten: {pellet.name} in Maze {MazeNumber}");
    //     pellet.gameObject.SetActive(false);
    //     SetScore(score + pellet.points);

    //     if (isMusicActive && audioManager != null)
    //     {
    //         if (isPowerPellet)
    //             audioManager.Play("powerpellet");
    //         else
    //             audioManager.Play("pellet");
    //     }

    //     bool remainingPellets = HasRemainingPellets();
    //     Debug.Log($"🔄 After eating pellet: {(remainingPellets ? "Still has pellets" : "No pellets remaining")}");

    //     if (!remainingPellets)
    //     {
    //         Debug.Log($"🎉 Level {MazeNumber} completed! Moving to next level...");
            
    //         pacman.gameObject.SetActive(false);
    //         SaveData(MazeNumber, NumberOfGhosts, GhostSpeed);

    //         if (GameEnds)
    //             GameStartsAgain();

    //         // Always show completion UI
    //         if (MoveingNextLevelTextPanel != null)
    //         {
    //             MoveingNextLevelTextPanel.SetActive(true);
    //             Debug.Log("✅ MoveingNextLevelTextPanel activated");
    //         }
    //         if (congratulationsText != null)
    //         {
    //             congratulationsText.SetActive(true);
    //             Debug.Log("✅ Congratulations text activated");
    //         }

    //         timeStop = true;
    //         Debug.Log($"⏰ Invoking AdvanceToNextMaze in {MoveingNextLevelTime} seconds");
    //         Invoke(nameof(AdvanceToNextMaze), MoveingNextLevelTime);
    //     }
    // }



    void GameStartsAgain()
    {
        GameEnds = false;
        MazeNumber = 0;
        mazeLoopPoint = 0;
        GhostSpeed = 3f;
    }

    public void PowerPelletEaten(PowerPellet pellet)
    {
        Debug.Log($"🔵 Power pellet eaten! Frightening {NumberOfGhosts} ghosts for {pellet.duration} seconds");

        for (int i = 0; i < NumberOfGhosts && i < ghosts.Length; i++)
        {
            if (ghosts[i] == null)
            {
                Debug.LogWarning($"⚠️ Ghost {i} is NULL in GameManager.ghosts[]!");
                continue;
            }

            if (!ghosts[i].gameObject.activeSelf)
            {
                Debug.LogWarning($"⚠️ Ghost {i} ({ghosts[i].name}) is inactive in scene!");
                continue;
            }

            if (ghosts[i].isRespawning)
            {
                Debug.LogWarning($"⚠️ Ghost {i} ({ghosts[i].name}) is respawning, skipping frighten!");
                continue;
            }

            if (ghosts[i].frightened == null)
            {
                Debug.LogError($"❌ Ghost {i} ({ghosts[i].name}) has NO GhostFrightened component assigned!");
                continue;
            }

            Debug.Log($"✅ Calling Frightened.Enable on ghost {i}: {ghosts[i].name}");
            ghosts[i].frightened.Enable(pellet.duration);
        }

        PelletEaten(pellet, true);

        CancelInvoke(nameof(ResetGhostMultiplier));
        Invoke(nameof(ResetGhostMultiplier), pellet.duration);
    }

    private bool HasRemainingPellets()
    {
        // Make sure `pellets` is updated to the current maze’s pellet container
        // whenever SetupCurrentScene() loads a new maze.
        if (pellets == null)
        {
            Debug.LogError("❌ pellets reference is null!");
            return true; // return true so game won’t advance
        }

        int activePellets = 0;
        foreach (Transform child in pellets.GetComponentsInChildren<Transform>(true))
        {
            if (child.CompareTag("Pellet") && child.gameObject.activeSelf)
                activePellets++;
        }

        Debug.Log($"📊 Active pellets in Maze {MazeNumber}: {activePellets}");
        return activePellets > 0;
    }






    private void ResetGhostMultiplier()
    {
        ghostMultiplier = 1;
        Debug.Log("Ghost multiplier reset to 1");
    }

    public void OnClickPaused()
    {
        Debug.Log("🛑 OnClickPaused called!");
        if (!paused)
        {
            Time.timeScale = 0;

            foreach (var item in CamMovingObjects)
                item.SetActive(false);

            PausedScreen.SetActive(true);

            // Re-enable PausePanelScript so controller works
            var pauseScript = PausedScreen.GetComponent<PausePanelScript>();
            if (pauseScript != null)
            {
                pauseScript.enabled = true;
                pauseScript.OnPanelActivated(); // custom setup function
            }

            if (pacman != null)
                pacman.gameObject.SetActive(false);

            paused = true;
        }
    }

    void ResetResume()
    {
        Time.timeScale = 1;
        foreach (var item in CamMovingObjects)
            item.SetActive(true);
        if (pacman != null)
            pacman.gameObject.SetActive(true);
        PausedScreen.SetActive(false);
        paused = false;
    }

    public void OnClickResume()
    {
        if (paused && audioManager != null)
        {
            audioManager.Play("select");
            StartCoroutine(WaitAfterResumingGame(ResumeDuration));
        }
    }

    public void ApplyRotations(Vector3 _Rotation)
    {
        foreach (var Rotated in RotatedGameObjects)
        {
            if (Rotated != null)
                Rotated.rotation = Quaternion.Euler(_Rotation);
        }
    }

    IEnumerator WaitAfterResumingGame(float duration)
    {
        Time.timeScale = 1;
        if (pacman != null)
            pacman.gameObject.SetActive(true);
        PausedScreen.SetActive(false);
        ResumeCountDownObject.SetActive(true);
        GhostStatus(false);
        yield return new WaitForSeconds(duration);
        GhostStatus(true);
        foreach (var item in CamMovingObjects)
            item.SetActive(true);
        paused = false;
    }

    void GhostStatus(bool enable)
    {
        // Only affect active ghosts
        for (int i = 0; i < NumberOfGhosts && i < ghosts.Length; i++)
        {
            if (ghosts[i] != null && ghosts[i].gameObject.activeSelf)
            {
                ghosts[i].agent.enabled = enable;
            }
        }
    }

    public void OnClickExit()
    {
        Time.timeScale = 1;
        if (audioManager != null)
            audioManager.Play("select");
        SceneManager.LoadScene("ui scene");
    }
}