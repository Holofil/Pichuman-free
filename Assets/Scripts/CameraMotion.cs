using UnityEngine;
using Cinemachine;

public class CameraMotion : MonoBehaviour
{
    [Header("Camera References")]
    public CinemachineVirtualCamera virtualCamera;   // Child VCAM
    public CinemachineSmoothPath path;               // Dolly path

    [Header("Player References")]
    public Movement movement;                        // Pacman Movement script

    [Header("Dolly Settings")]
    public float cameraTransitionSpeed = 0.65f;

    private CinemachineTrackedDolly trackedDolly;
    private float targetPathPosition = 0f;
    private bool isTransitioning = false;
    private short currentArea = 1;

    // ===== FLOATING CAMERA TILT (TiltRig rotation) =====
    [Header("Floating Tilt Settings")]
    public bool enableTilt = true;
    public float maxTiltAngle = 4f;
    public Vector2 tiltInterval = new Vector2(0.35f, 0.9f);
    public Vector2 tiltSpeedRange = new Vector2(12f, 24f);
    public float minTiltAngle = 0.8f;
    [Range(0f, 1f)]
    public float singleAxisBias = 0.65f;

    private Quaternion baseRotation;     // Initial rotation of TILTRIG
    private Quaternion tiltTarget;       // Next tilt destination
    private float tiltSpeed = 10f;
    private float nextTiltTime = 0f;

    private void Start()
    {
        if (virtualCamera == null || path == null || movement == null)
        {
            Debug.LogError("[CameraMotion] Missing references! virtualCamera, path and movement are required.");
            return;
        }

        // Get Dolly brain from the VCAM
        trackedDolly = virtualCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
        if (trackedDolly == null)
        {
            Debug.LogError("[CameraMotion] Virtual Camera is missing a TrackedDolly component!");
            return;
        }

        // initialize dolly position
        trackedDolly.m_PathPosition = 0f;
        targetPathPosition = 0f;

        // Store initial rotation of TILTRIG (this GameObject)
        baseRotation = transform.localRotation;

        // Start with no tilt
        tiltTarget = Quaternion.identity;
        tiltSpeed = Random.Range(tiltSpeedRange.x, tiltSpeedRange.y);
        nextTiltTime = Time.time + 0.25f;
    }

    private void Update()
    {
        if (movement == null || trackedDolly == null)
            return;

        HandleDollyTransitions();
        ApplyFloatingTilt();
    }

    // ============================================================
    //                  DOLLY PATH MOVEMENT
    // ============================================================
    private void HandleDollyTransitions()
    {
        float z = movement.transform.position.z;
        short newArea = currentArea;

        // PLAYER ENTERS TOP MAZE
        if (z > 4.3f && targetPathPosition != path.PathLength)
        {
            targetPathPosition = path.PathLength;
            isTransitioning = true;
            newArea = 2;
        }

        // PLAYER RETURNS TO BOTTOM MAZE
        else if (z <= 3.5f && targetPathPosition != 0f)
        {
            targetPathPosition = 0f;
            isTransitioning = true;
            newArea = 1;
        }

        // Notify Movement of area change
        if (newArea != currentArea)
        {
            currentArea = newArea;
            movement.SetAreaNumber(currentArea);
            Debug.Log($"[CameraMotion] Area changed to {currentArea}");
        }

        // Smoothly move along path
        if (isTransitioning)
        {
            float newPos = Mathf.MoveTowards(
                trackedDolly.m_PathPosition,
                targetPathPosition,
                cameraTransitionSpeed * Time.deltaTime
            );

            trackedDolly.m_PathPosition = newPos;

            if (Mathf.Abs(newPos - targetPathPosition) < 0.01f)
                isTransitioning = false;
        }
    }

    // Public helpers expected by Movement.cs
    public void MoveToPos1()
    {
        TransitionToPosition(0f);
    }

    public void MoveToPos2()
    {
        if (path == null)
            return;
        TransitionToPosition(path.PathLength);
    }

    private void TransitionToPosition(float pos)
    {
        if (trackedDolly == null)
            return;

        if (Mathf.Abs(trackedDolly.m_PathPosition - pos) > 0.01f)
        {
            targetPathPosition = pos;
            isTransitioning = true;
        }
    }

    // ============================================================
    //                     FLOATING CAMERA TILT
    // ============================================================
    private void ApplyFloatingTilt()
    {
        if (!enableTilt)
            return;

        // Current rig rotation
        Quaternion currentRot = transform.localRotation;
        Quaternion desiredRot = baseRotation * tiltTarget;

        // Need a new tilt target?
        if (Time.time >= nextTiltTime || Quaternion.Angle(currentRot, desiredRot) < 0.2f)
        {
            float ax = Random.Range(minTiltAngle, maxTiltAngle) * (Random.value < 0.5f ? -1f : 1f);
            float az = Random.Range(minTiltAngle, maxTiltAngle) * (Random.value < 0.5f ? -1f : 1f);

            // Sometimes tilt only one axis
            if (Random.value < singleAxisBias)
            {
                if (Random.value < 0.5f) ax = 0f;
                else az = 0f;
            }

            tiltTarget = Quaternion.Euler(ax, 0f, az);

            tiltSpeed = Random.Range(tiltSpeedRange.x, tiltSpeedRange.y);
            nextTiltTime = Time.time + Random.Range(tiltInterval.x, tiltInterval.y);
        }

        // Apply tilt to TILTRIG, NOT VirtualCamera
        transform.localRotation = Quaternion.RotateTowards(
            currentRot,
            desiredRot,
            tiltSpeed * Time.deltaTime
        );
    }
}
