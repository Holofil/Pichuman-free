using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostMovement : MonoBehaviour
{
    [Header("Movement")]
    public float SpeedMultiplayer = 1f;
    [SerializeField] Vector3 InitialDirection;
    [SerializeField] LayerMask ObstacleLayer; // Set this to "Ground" in Unity Inspector
    [SerializeField] float HalfExtend = 0.75f;
    [SerializeField] float MaxDistance = 1.5f;
    [SerializeField] float TurnSmoothTime = 0.1f;

    [Header("Sine up down movement")]
    [SerializeField] Transform GhostObject;
    [SerializeField] float Amplitude = 0.5f;
    [SerializeField] float Frequency = 0.1f;

    [Header("Shooting")]
    [SerializeField] GameObject BulletPrefab;
    [SerializeField] float TimeToShootBullet;
    [SerializeField] Transform ShootingPoint;

    public Rigidbody rigidBody { get; private set; }

    public Vector3 Direction { get; private set; }

    public Vector3 NextDirection { get; private set; }

    public Vector3 StartingPosition { get; set; }

    Ghost ghost;
    float turnSmoothVelocity;
    float ShootingTime;
    bool HitDetect;
    RaycastHit RayHit;
    GameManager gameManager;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        ghost = GetComponent<Ghost>();
        gameManager = FindObjectOfType<GameManager>();
    }

    private void Start()
    {
        ResetState();
    }

    public void ResetState()
    {
        SpeedMultiplayer = 1;
        Direction = InitialDirection;
        NextDirection = Vector3.zero;
        transform.position = StartingPosition;
        Amplitude = Random.Range(0.12f, 0.16f);
        Frequency = Random.Range(1.5f, 2f);
        ShootingTime = TimeToShootBullet;
        rigidBody.isKinematic = false;
        this.enabled = true;
    }

    private void Update()
    {
        if (NextDirection != Vector3.zero)
        {
            SetDirection(NextDirection);
        }

        if (Direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(Direction.x, Direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, TurnSmoothTime);
            transform.rotation = Quaternion.Euler(0, angle, 0);
        }

        GhostObject.localPosition = new Vector3(0, Mathf.Sin(Time.time * Frequency) * Amplitude, 0);
    }

    public void SetDirection(Vector3 direction, bool forced = false)
    {
        HitDetect = Occupied(direction);
        if (forced || !HitDetect)
        {
            Direction = direction;
            NextDirection = Vector3.zero;
        }
        else
        {
            NextDirection = direction;
        }
    }

    public bool SetNewDirection(Vector3 direction)
    {
        if (!Occupied(direction))
        {
            Direction = direction;
            NextDirection = Vector3.zero;
            return true;
        }
        else
        {
            NextDirection = direction;
        }
        return false;
    }

    public bool Occupied(Vector3 direction)
    {
        return Physics.BoxCast(transform.position, Vector3.one * HalfExtend, direction, out RayHit, Quaternion.identity, MaxDistance, LayerMask.GetMask("Ground"));
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
    }
}