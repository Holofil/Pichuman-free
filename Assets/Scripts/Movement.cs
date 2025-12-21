using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour
{
    [SerializeField] CameraMotion cameraMotion;
    [SerializeField] float speed = 8.0f;
    [SerializeField] float SpeedMultiplayer = 1f;
    [SerializeField] LayerMask ObstacleLayer;
    [SerializeField] float HalfExtend = 0.3f;
    [SerializeField] float MaxDistance = .3f;
    [SerializeField] float InputBufferTime = 0.7f;

    public short AreaNumber = 1;
    public Rigidbody rigidBody { get; private set; }
    public Vector3 Direction { get; private set; }
    public Vector3 NextDirection { get; private set; }
    public Vector3 StartingPosition { get; private set; }
    public Animator animator { get; private set; }
    public GameManager gameManager { get; private set; }

    // Input queue system
    private Queue<Vector3> inputQueue = new Queue<Vector3>();
    private float lastInputTime;
    private Vector3 lastProcessedInput = Vector3.zero;
    
    // Persistent retry system - KEY IMPROVEMENT
    private Vector3 pendingDirection = Vector3.zero;
    private float pendingDirectionTime = 0f;
    private const float PENDING_TIMEOUT = 1.0f; // Clear pending after 1 second

    bool HitDetect;
    RaycastHit RayHit;
    Vector3 previousPos = Vector3.zero;
    float turnSmoothVelocity;
    float TurnSmoothTime = 0.1f;

    public bool InvertControls => AreaNumber == 2;

    // Swipe variables
    Vector2 touchStart;
    bool isSwiping = false;
    const float swipeThreshold = 50f;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("Animator not found on Pacman! Make sure it's attached.");
        }

        gameManager = FindObjectOfType<GameManager>();
        StartingPosition = transform.position;
    }

    private void Start()
    {
        ResetState();
    }

    public void ResetState()
    {
        SpeedMultiplayer = 1;
        Direction = Vector3.zero;
        NextDirection = Vector3.zero;
        transform.position = StartingPosition;
        rigidBody.isKinematic = false;
        this.enabled = true;

        // Clear ALL input states on reset
        inputQueue.Clear();
        lastInputTime = 0f;
        lastProcessedInput = Vector3.zero;
        pendingDirection = Vector3.zero;
        pendingDirectionTime = 0f;

        if (animator != null)
            animator.SetBool("running4pichu", false);

        previousPos = transform.position;

        // ❌ Remove this entire section:
        // Direction = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        // if (Direction.magnitude < 0.1f)
        //     Direction = Vector3.forward;

        Debug.Log("[Movement] State fully reset - waiting for player input");
    }


    private void Update()
    {
        if (!gameManager.paused)
        {
            HandleInput();
            ProcessInputQueue();
            CheckPendingDirection(); // NEW: Continuously check pending direction
            CameraSetup();

            if (Direction.magnitude >= 0.1f)
            {
                float targetAngle = Mathf.Atan2(Direction.x, Direction.z) * Mathf.Rad2Deg;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, TurnSmoothTime);
                transform.rotation = Quaternion.Euler(0, angle, 0);
            }
        }
    }

    // Called by CameraMotion when area changes
    public void SetAreaNumber(short newArea)
    {
        if (AreaNumber != newArea)
        {
            Debug.Log($"[Movement] Area changed from {AreaNumber} to {newArea} - Controls {(newArea == 2 ? "INVERTED" : "NORMAL")}");
            AreaNumber = newArea;
        }
    }

    // NEW: Persistent check for pending direction
    private void CheckPendingDirection()
    {
        if (pendingDirection == Vector3.zero)
            return;

        // Clear pending if it's too old
        if (Time.time - pendingDirectionTime > PENDING_TIMEOUT)
        {
            Debug.Log($"[Movement] Pending direction timed out: {pendingDirection}");
            pendingDirection = Vector3.zero;
            return;
        }

        // Try to apply pending direction every frame
        if (!Occupied(pendingDirection))
        {
            Debug.Log($"[Movement] Pending direction cleared, applying: {pendingDirection}");
            SetDirectionImmediate(pendingDirection);
            pendingDirection = Vector3.zero;
        }
    }

    private void HandleInput()
    {
        Vector3 inputDirection = Vector3.zero;

        // ✅ 1. CONTROLLER (highest priority) — continuous HatSwitch / D-Pad polling
        var gp = Gamepad.current;
        if (gp != null)
        {
            Vector2 dpad = gp.dpad.ReadValue();

            if (dpad.y > 0.5f)
                inputDirection = Vector3.forward;
            else if (dpad.y < -0.5f)
                inputDirection = Vector3.back;
            else if (dpad.x < -0.5f)
                inputDirection = Vector3.left;
            else if (dpad.x > 0.5f)
                inputDirection = Vector3.right;
        }

        // ✅ 2. KEYBOARD (only if controller idle)
        if (inputDirection == Vector3.zero)
        {
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed)
                    inputDirection = Vector3.forward;
                else if (kb.sKey.isPressed || kb.downArrowKey.isPressed)
                    inputDirection = Vector3.back;
                else if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)
                    inputDirection = Vector3.left;
                else if (kb.dKey.isPressed || kb.rightArrowKey.isPressed)
                    inputDirection = Vector3.right;
            }
        }

        // ✅ 3. TOUCH / SWIPE
        HandleTouchInput(ref inputDirection);

        // ✅ 4. Send to movement queue
        if (inputDirection != Vector3.zero)
            QueueInput(inputDirection);
    }


    private void HandleTouchInput(ref Vector3 inputDirection)
    {
        var ts = Touchscreen.current;
        if (ts == null) return;

        var touch = ts.primaryTouch;
        if (touch == null) return;

        if (touch.press.wasPressedThisFrame)
        {
            touchStart = touch.position.ReadValue();
            isSwiping = true;
            return;
        }

        if (isSwiping && touch.press.isPressed)
        {
            Vector2 currentPos = touch.position.ReadValue();
            Vector2 delta = currentPos - touchStart;

            if (delta.magnitude > swipeThreshold)
            {
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                {
                    inputDirection = delta.x > 0 ? Vector3.right : Vector3.left;
                }
                else
                {
                    inputDirection = delta.y > 0 ? Vector3.forward : Vector3.back;
                }

                isSwiping = false;
            }
            return;
        }

        if (touch.press.wasReleasedThisFrame)
        {
            isSwiping = false;
        }
    }

    private void QueueInput(Vector3 inputDirection)
    {
        Vector3 processedDirection = ApplyControlInversion(inputDirection);

        // Same input as last - refresh timer
        if (processedDirection == lastProcessedInput)
        {
            lastInputTime = Time.time;
            if (!inputQueue.Contains(processedDirection))
                inputQueue.Enqueue(processedDirection);
            return;
        }

        // Immediate opposite direction change
        if (Direction != Vector3.zero && processedDirection == -Direction)
        {
            inputQueue.Clear();
            SetDirectionImmediate(processedDirection);
            lastProcessedInput = processedDirection;
            pendingDirection = Vector3.zero; // Clear pending since we're moving
            Debug.Log($"[Movement] Immediate opposite direction change: {processedDirection}");
            return;
        }

        // Replace last queued if opposite
        if (inputQueue.Count > 0)
        {
            Vector3 lastQueuedDirection = inputQueue.ToArray()[inputQueue.Count - 1];
            if (processedDirection == -lastQueuedDirection)
            {
                Queue<Vector3> tempQueue = new Queue<Vector3>();
                Vector3[] queueArray = inputQueue.ToArray();

                for (int i = 0; i < queueArray.Length - 1; i++)
                {
                    tempQueue.Enqueue(queueArray[i]);
                }

                inputQueue = tempQueue;
                inputQueue.Enqueue(processedDirection);
                lastInputTime = Time.time;
                lastProcessedInput = processedDirection;

                Debug.Log($"[Movement] Replaced last queued direction with opposite: {processedDirection}");
                return;
            }
        }

        // Add to queue if not already there
        if (!inputQueue.Contains(processedDirection))
        {
            inputQueue.Enqueue(processedDirection);
            lastInputTime = Time.time;
            lastProcessedInput = processedDirection;

            Debug.Log($"[Movement] Queued input: {processedDirection}, Queue size: {inputQueue.Count}");
        }
    }

    private void ProcessInputQueue()
    {
        // Clear old inputs
        if (Time.time - lastInputTime > InputBufferTime)
        {
            inputQueue.Clear();
        }

        // Process queued inputs
        while (inputQueue.Count > 0)
        {
            Vector3 queuedDirection = inputQueue.Peek();

            if (!Occupied(queuedDirection))
            {
                SetDirectionImmediate(queuedDirection);
                inputQueue.Dequeue();
                pendingDirection = Vector3.zero; // Clear pending since we applied it
                Debug.Log($"[Movement] Applied queued direction: {queuedDirection}");
                break;
            }
            else
            {
                // IMPROVED: Set as pending for continuous retry
                if (pendingDirection != queuedDirection)
                {
                    pendingDirection = queuedDirection;
                    pendingDirectionTime = Time.time;
                    Debug.Log($"[Movement] Direction blocked, set as pending: {queuedDirection}");
                }
                
                inputQueue.Dequeue(); // Remove from queue to prevent buildup
                break;
            }
        }
    }

    private Vector3 ApplyControlInversion(Vector3 inputDirection)
    {
        if (!InvertControls)
            return inputDirection;

        if (inputDirection == Vector3.forward)
            return Vector3.back;
        else if (inputDirection == Vector3.back)
            return Vector3.forward;
        else if (inputDirection == Vector3.left)
            return Vector3.right;
        else if (inputDirection == Vector3.right)
            return Vector3.left;

        return inputDirection;
    }

    private void FixedUpdate()
    {
        if (!gameManager.paused)
        {
            if (!Occupied(Direction))
            {
                Vector3 position = rigidBody.position;
                Vector3 translation = Direction * speed * SpeedMultiplayer * Time.fixedDeltaTime;
                rigidBody.MovePosition(position + translation);
            }

            AnimationSetup();
        }
    }

    void AnimationSetup()
    {
        if (animator == null) return;

        bool moved = Vector3.Distance(previousPos, transform.position) > 0.001f;
        bool canMove = !Occupied(Direction);

        if (!moved || !canMove)
            animator.SetBool("running4pichu", false);
        else
            animator.SetBool("running4pichu", true);

        previousPos = transform.position;
    }

    void CameraSetup()
    {
        if (AreaNumber == 1 && transform.position.z <= 2f)
        {
            cameraMotion.MoveToPos1();
        }
        else if (AreaNumber == 2 && transform.position.z > 6f)
        {
            cameraMotion.MoveToPos2();
        }
    }

    public void ClearDirections()
    {
        Direction = Vector3.zero;
        NextDirection = Vector3.zero;
        inputQueue.Clear();
        pendingDirection = Vector3.zero; // NEW: Clear pending too
    }

    // IMPROVED: Separated immediate direction setting
    private void SetDirectionImmediate(Vector3 direction)
    {
        Direction = direction;
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, angle, 0);
        NextDirection = Vector3.zero;
    }

    public void SetDirection(Vector3 direction)
    {
        HitDetect = Occupied(direction);

        if (!HitDetect)
        {
            SetDirectionImmediate(direction);
            pendingDirection = Vector3.zero;
        }
        else
        {
            NextDirection = direction;
            // NEW: Set as pending for retry
            pendingDirection = direction;
            pendingDirectionTime = Time.time;
            Debug.Log($"[Movement] Direction blocked in SetDirection, set as pending: {direction}");
        }
    }

    // Public method for controller input (Pacman.cs compatibility)
    public void AddInputToQueue(Vector3 inputDirection)
    {
        QueueInput(inputDirection);
    }

    public bool Occupied(Vector3 direction)
    {
        if (direction == Vector3.zero)
            return false;

        // Already inside a wall
        if (Physics.CheckSphere(transform.position, HalfExtend - 0.05f, ObstacleLayer))
            return true;

        bool isTurning = Vector3.Dot(Direction.normalized, direction.normalized) < 0.1f && Direction != Vector3.zero;

        float predictiveDistance = MaxDistance;
        float detectionRadius;

        if (isTurning)
        {
            detectionRadius = HalfExtend * 1.0f;
            predictiveDistance *= 1.5f;
        }
        else
        {
            detectionRadius = HalfExtend * 0.8f;
        }

        // Multiple checks for turning
        if (isTurning)
        {
            Vector3 checkPos1 = transform.position;
            Vector3 checkPos2 = transform.position + direction.normalized * (HalfExtend * 0.5f);
            Vector3 checkPos3 = transform.position + direction.normalized * HalfExtend;

            if (Physics.CheckSphere(checkPos1, detectionRadius, ObstacleLayer) ||
                Physics.CheckSphere(checkPos2, detectionRadius, ObstacleLayer) ||
                Physics.CheckSphere(checkPos3, detectionRadius, ObstacleLayer))
            {
                return true;
            }
        }

        Vector3 castOrigin = transform.position + direction.normalized * 0.02f;

        return Physics.SphereCast(
            castOrigin,
            detectionRadius,
            direction.normalized,
            out RayHit,
            predictiveDistance,
            ObstacleLayer
        );
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        if (HitDetect)
        {
            Gizmos.DrawRay(transform.position, NextDirection * RayHit.distance);
            Gizmos.DrawWireSphere(transform.position + NextDirection * RayHit.distance, HalfExtend);
        }
        else
        {
            Gizmos.DrawRay(transform.position, NextDirection * MaxDistance);
            Gizmos.DrawWireSphere(transform.position + NextDirection * MaxDistance, HalfExtend);
        }

        // NEW: Visualize pending direction
        if (pendingDirection != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, pendingDirection * 0.5f);
        }
    }
}