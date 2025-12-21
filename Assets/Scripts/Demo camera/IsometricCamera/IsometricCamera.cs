using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsometricCamera : AbstractCamera
{
    private Camera _camera;
    private List<GameObject> _targetObjects = new List<GameObject>();
    private float _minX = 0f;
    private float _maxX = 0f;
    private float _minY = 0f;
    private float _maxY = 0f;
    private float _minZ = 0f;
    private float _maxZ = 0f;
    private Vector3 _localPosition = new Vector3(0, 0, -20);
    private IsometricCameraData _data;
    private Vector3 _posWorldSpace = new Vector3(0, 0, 0);

    // Use this for initialization
    void Start()
    {
        _data = Resources.Load<IsometricCameraData>("IsometricCameraData");
        _camera = GetComponent<Camera>();
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //transform.eulerAngles = _data.Rotation;
        CleanUpObjects();

        if (_targetObjects.Count < 1)
            return;

        var pos = _targetObjects[0].transform.position;
        _minX = pos.x;
        _maxX = pos.x;
        _minY = pos.y;
        _maxY = pos.y;
        _minZ = pos.z;
        _maxZ = pos.z;

        for (var i = 0; i < _targetObjects.Count; i++)
        {
            pos = _targetObjects[i].transform.position;
            _maxX = Mathf.Max(_maxX, pos.x);
            _minX = Mathf.Min(_minX, pos.x);
            _maxY = Mathf.Max(_maxY, pos.y);
            _minY = Mathf.Min(_minY, pos.y);
            _maxZ = Mathf.Max(_maxZ, pos.z);
            _minZ = Mathf.Min(_minZ, pos.z);
        }

        //Calculate the center position between the targets and lerp the camera position towards the center
        var centerWorldSpace = new Vector3((_minX + _maxX) / 2, (_minY + _maxY) / 2, (_minZ + _maxZ) / 2);
        _posWorldSpace = Vector3.Lerp(_posWorldSpace, centerWorldSpace, _data.MoveSpeed * Time.deltaTime);

        //Calculate optimal camera size based on targets
        //Determine the local space positions of the camera corners
        var cameraCorners = new Corners();
        var plane = new Plane(transform.rotation * Vector3.back, _posWorldSpace);
        float distance;
        var ray = _camera.ViewportPointToRay(Vector3.zero);
        if (plane.Raycast(ray, out distance))
            cameraCorners.DownLeft = transform.InverseTransformPoint(ray.GetPoint(distance));
        ray = _camera.ViewportPointToRay(Vector3.up);
        if (plane.Raycast(ray, out distance))
            cameraCorners.UpLeft = transform.InverseTransformPoint(ray.GetPoint(distance));
        ray = _camera.ViewportPointToRay(Vector3.right);
        if (plane.Raycast(ray, out distance))
            cameraCorners.DownRight = transform.InverseTransformPoint(ray.GetPoint(distance));
        ray = _camera.ViewportPointToRay(Vector3.one);
        if (plane.Raycast(ray, out distance))
            cameraCorners.UpRight = transform.InverseTransformPoint(ray.GetPoint(distance));

        //Determine with which value the camera size has to be multiplied to fit all targets in the screen
        float max = -99;
        foreach (var target in _targetObjects)
        {
            //Transform the target position to local space to compare it to the camera corners
            var position = transform.InverseTransformPoint(target.transform.position);
            var targetX = position.x;
            var targetY = position.y;

            //The calculate below can be explained as follows
            //Compare the minimum and maximum values on a certain axis to the value of the target
            //determine which value the camera size has to be multiplied with to get this point with the min and max axis value of the camera
            //This is done 4 times for the min and max values of the camera for both the axis x and z
            var tempScale = ((((_data.ScaleOffset + targetX) - cameraCorners.RightHorizontalValue) * 2) + cameraCorners.GetWidth()) / cameraCorners.GetWidth();
            if (tempScale > max)
                max = tempScale;

            tempScale = (((cameraCorners.LeftHorizontalValue - (targetX - _data.ScaleOffset)) * 2) + cameraCorners.GetWidth()) / cameraCorners.GetWidth();
            if (tempScale > max)
                max = tempScale;

            tempScale = (((cameraCorners.BottomVerticalValue - (targetY - _data.ScaleOffset)) * 2) + cameraCorners.GetHeight()) / cameraCorners.GetHeight();
            if (tempScale > max)
                max = tempScale;

            tempScale = ((((targetY + _data.ScaleOffset) - cameraCorners.TopVecticalValue) * 2) + cameraCorners.GetHeight()) / cameraCorners.GetHeight();
            if (tempScale > max)
                max = tempScale;
        }

        //Lerp camera towards optimal screen size only within the certain threshold
        if (max > 1f || max < (1f - _data.Treshold))
        {
            var target = _localPosition.z * max;
            var speed = (target > _localPosition.z) ? _data.ZoomInSpeed : _data.ZoomOutSpeed;
            _localPosition.z = Mathf.Lerp(_localPosition.z, target, speed * Time.deltaTime);
        }

        //Camera can't scale below minimum size
        if (_localPosition.z > -_data.MinSize)
            _localPosition.z = -_data.MinSize;

        //Transform local position back to world space and set it as the new camera position
        transform.position = _posWorldSpace + ((transform.rotation) * _localPosition);
    }

    /// <summary>
    /// Remove all empty targets
    /// </summary>
    void CleanUpObjects()
    {
        _targetObjects.RemoveAll(p => p == null);
    }

    public override void AddTarget(Transform obj)
    {
        if (_targetObjects.Contains(obj.gameObject))
            return;

        _targetObjects.Add(obj.gameObject);
    }

    /// <summary>
    /// Holds information about camera corners and differences
    /// </summary>
    private class Corners
    {
        public Vector3 DownLeft;
        public Vector3 DownRight;
        public Vector3 UpLeft;
        public Vector3 UpRight;

        /// <summary>
        /// Returns the smallest left side X value
        /// </summary>
        public float LeftHorizontalValue
        {
            get { return Mathf.Max(DownLeft.x, UpLeft.x); }
        }

        /// <summary>
        /// Returns the biggest right side X value
        /// </summary>
        public float RightHorizontalValue
        {
            get { return Mathf.Min(DownRight.x, UpRight.x); }
        }

        /// <summary>
        /// Returns the smallest bottom side Y value
        /// </summary>
        public float BottomVerticalValue
        {
            get { return Mathf.Max(DownLeft.y, DownRight.y); }
        }

        /// <summary>
        /// Returns the biggest top side Y value
        /// </summary>
        public float TopVecticalValue
        {
            get { return Mathf.Min(UpLeft.y, UpRight.y); }
        }

        public float GetWidth()
        {

            return RightHorizontalValue - LeftHorizontalValue;
        }

        public float GetHeight()
        {

            return UpLeft.y - DownLeft.y;
        }

    }
}
