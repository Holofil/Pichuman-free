using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Movement))]
public class Pacman : MonoBehaviour
{
    private GameManager gameManager;
    public Movement movement { get; private set; }
    public new Collider collider { get; private set; }
    public ParticleSystem DieEffect;
    public float ParticlePauseTime = 1f;

    private float controllerInputCooldown = 0.3f; // short delay after respawn
    private float controllerInputTimer = 0f;

    Controllers _controller;
    Controllers Controller
    {
        get
        {
            if (_controller == null)
                _controller = new Controllers();
            return _controller;
        }
    }

    bool isController = false;
    bool HolographicController = false;

    // Store last direction to prevent flooding Movement queue
    private Vector3 lastQueuedDirection = Vector3.zero;
    private float controllerDeadZone = 0.3f; // Avoid small joystick noise

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        collider = GetComponent<Collider>();
        movement = GetComponent<Movement>();

        // Remove unreliable performed-based control and pause setup remains fine
        Controller.Gamepad.ButtonLeft.canceled += ButtonLeft_canceled;

        Debug.Log("[Pacman] Awake - Initial AreaNumber: " + movement.AreaNumber);
    }

    private void Start()
    {
        if (HolographicController)
        {
            movement.gameManager.ApplyRotations(new Vector3(0f, -180f, 0f));
        }
        else
        {
            movement.gameManager.ApplyRotations(new Vector3(0f, 0f, 0f));
        }
        Debug.Log("[Pacman] Start - AreaNumber: " + movement.AreaNumber);
    }

    private void ButtonLeft_canceled(InputAction.CallbackContext obj)
    {
        movement.gameManager.OnClickPaused();
    }

    // ✅ Continuous controller polling (fixes Android issue)
    private void Update()
    {
        if (!isController || movement.gameManager.paused)
            return;

        // Small cooldown after respawn to avoid stale joystick direction
        if (controllerInputTimer > 0f)
        {
            controllerInputTimer -= Time.deltaTime;
            return;
        }

        var stick = Controller.Gamepad.Movement.ReadValue<Vector2>();

        // Ignore small noise
        if (stick.magnitude < controllerDeadZone)
            return;

        Vector3 inputDirection = ConvertInputToDirection(stick);
        if (inputDirection == Vector3.zero)
            return;

        // Apply holographic flip if needed
        if (HolographicController)
        {
            if (inputDirection == Vector3.right)
                inputDirection = Vector3.left;
            else if (inputDirection == Vector3.left)
                inputDirection = Vector3.right;
        }

        // Avoid spamming same direction
        if (inputDirection != lastQueuedDirection)
        {
            movement.AddInputToQueue(inputDirection);
            lastQueuedDirection = inputDirection;
            Debug.Log("[Pacman] Controller Queued: " + inputDirection);
        }
    }


    private Vector3 ConvertInputToDirection(Vector2 input)
    {
        // Convert 2D input to 3D direction
        if (input.x > 0.5f)
            return Vector3.right;
        else if (input.x < -0.5f)
            return Vector3.left;
        else if (input.y > 0.5f)
            return Vector3.forward;
        else if (input.y < -0.5f)
            return Vector3.back;

        return Vector3.zero;
    }

    private void OnEnable()
    {
        string mode = PlayerPrefs.GetString("Mode", "Touch");
        if (mode == "Touch")
        {
            isController = false;
            HolographicController = false;
        }
        else if (mode == "Controller")
        {
            isController = true;
            HolographicController = false;
        }
        else if (mode == "Holographic")
        {
            isController = true;
            HolographicController = true;
        }

        if (isController)
            Controller.Enable();
    }

    private void OnDisable()
    {
        if (isController)
            Controller.Disable();
    }

    public void ResetState()
    {
        enabled = true;
        collider.enabled = true;

        // Reposition Pacman to his spawn location from GameManager
        if (gameManager != null)
        {
            transform.position = gameManager.MazeData[gameManager.MazeNumber].PacmanPos;
            Debug.Log($"⏪ Pacman repositioned to spawn: {transform.position}");
        }
        else
        {
            Debug.LogWarning("⚠️ GameManager not found in Pacman.");
        }

        movement.ResetState();
        gameObject.SetActive(true);
        controllerInputTimer = controllerInputCooldown;
        lastQueuedDirection = Vector3.zero;

    }

    ParticleSystem _dieEffect;

    public void DeathSequence()
    {
        if (movement.gameManager.isGraphicsActive)
            PacmanDieEffect();
        enabled = false;
        collider.enabled = false;
        movement.enabled = false;
        gameObject.SetActive(false);
        movement.animator.SetBool("running4pichu", false);

        Invoke(nameof(DeactivatePlayer), 0.1f);
    }

    void DeactivatePlayer()
    {
        gameObject.SetActive(false);
    }

    void PacmanDieEffect()
    {
        _dieEffect = Instantiate(DieEffect, transform.position, Quaternion.identity);
        _dieEffect.Play();
        Invoke(nameof(DieEffectPaused), ParticlePauseTime);
        Destroy(_dieEffect.gameObject, movement.gameManager.PacmanReturnTime);
    }

    void DieEffectPaused()
    {
        _dieEffect.Pause(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Pacman collided with: {other.gameObject.name}, Layer: {other.gameObject.layer}, Tag: {other.tag}");
        if (other.CompareTag("Ghost"))
        {
            Ghost ghost = other.GetComponent<Ghost>();

            if (ghost != null && ghost.frightened.enabled)
            {
                Debug.Log("Pac-Man eating frightened ghost: " + ghost.gameObject.name);
                GameManager gameManager = FindObjectOfType<GameManager>();
                if (gameManager != null)
                {
                    gameManager.GhostEaten(ghost);
                }
            }
            else if (ghost != null && !ghost.frightened.enabled)
            {
                Debug.Log("Pac-Man hit by normal ghost: " + ghost.gameObject.name);
                GameManager gameManager = FindObjectOfType<GameManager>();
                if (gameManager != null)
                {
                    gameManager.PacmanEaten();
                }
            }
        }
    }
}
