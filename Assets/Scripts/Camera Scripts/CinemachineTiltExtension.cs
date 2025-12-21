using UnityEngine;
using Cinemachine;

[ExecuteAlways]
[AddComponentMenu("Cinemachine/Extensions/Floating Camera Effect")]
public class CinemachineTiltExtension : CinemachineExtension
{
    [Header("1. Horizontal Sway (Left/Right)")]
    public float swaySpeed = 0.5f;
    public float swayAmount = 1.0f;

    public float centeringOffset = 0.0f;
    public bool useWorldSpaceSway = true;

    [Header("2. Vertical Bob (Up/Down Floating)")]
    public float bobSpeed = 0.8f;
    public float bobAmount = 0.8f;

    [Header("3. Permanent Upward Lift (Boss Request)")]
    public float permanentUpOffset = 0.5f;  // << REQUIRED FIX

    [Header("4. Rotational Effects")]
    public bool enableLookAt = true;
    public Vector3 lookAtTarget = Vector3.zero;

    public float bankingAmount = -3.0f;
    public float nodAmount = 2.0f;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        // === IMPORTANT FIX ===
        // Apply permanent camera height offset BEFORE sway/bob.
        // This offset happens IN CAMERA SPACE, not corrected by Composer.
        if (stage == CinemachineCore.Stage.Body)
        {
            Vector3 liftedPos = state.RawPosition;
            liftedPos.y += permanentUpOffset;     // <-- THIS IS THE MAGIC FIX
            state.RawPosition = liftedPos;
        }

        if (stage != CinemachineCore.Stage.Body)
            return;

        // ----- 1. Horizontal Sway -----
        float rawSway = Mathf.Cos(Time.time * swaySpeed) * swayAmount;
        float finalX = rawSway + centeringOffset;

        Vector3 swayMove = useWorldSpaceSway ?
            new Vector3(finalX, 0, 0) :
            state.RawOrientation * new Vector3(finalX, 0, 0);

        // ----- 2. Vertical Bob -----
        float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        Vector3 bobMove = new Vector3(0, bob, 0);

        // Apply floating movement
        state.PositionCorrection += swayMove + bobMove;


        // ----- 3. Rotation -----
        if (enableLookAt)
        {
            Vector3 finalPos = state.RawPosition + state.PositionCorrection;
            Vector3 dir = lookAtTarget - finalPos;
            Quaternion lookRot = Quaternion.LookRotation(dir);

            float zTilt = Mathf.Cos(Time.time * swaySpeed) * bankingAmount;
            float xNod = Mathf.Sin(Time.time * bobSpeed) * nodAmount;

            state.RawOrientation = lookRot * Quaternion.Euler(xNod, 0, zTilt);
        }
    }
}
