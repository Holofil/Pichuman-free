using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IsometricCameraData", menuName = "Data/IsometricCameraData")]
public class IsometricCameraData : ScriptableObject
{
    public float MoveSpeed = 4f;
    public float ZoomOutSpeed = 4f;
    public float ZoomInSpeed = .6f;
    public float Treshold = .1f;
    public float MinSize = 5f;
    public float ScaleOffset = 1f;
    public Vector3 Rotation = new Vector3(90, 0, 0);
}
