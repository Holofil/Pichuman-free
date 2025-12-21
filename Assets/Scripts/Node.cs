using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public float HalfExtend = 0.5f;
    public float MaxDistance = 1f;
    public LayerMask ObstacleLayer;

    public List<Vector3> availableDirections { get; private set; }

    private void Start()
    {
        availableDirections = new List<Vector3>();

        CheckAvailableDirection(Vector3.right);
        CheckAvailableDirection(Vector3.left);
        CheckAvailableDirection(Vector3.forward);
        CheckAvailableDirection(Vector3.back);
    }

    public void CheckAvailableDirection(Vector3 direction)
    {
        if(!Physics.BoxCast(transform.position, Vector3.one * HalfExtend, direction, Quaternion.identity, MaxDistance, ObstacleLayer))
            availableDirections.Add(direction);
    }

}
