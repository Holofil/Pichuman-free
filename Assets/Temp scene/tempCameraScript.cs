using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tempCameraScript : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] new string name;
    int i = 1;

    private void Update()
    {
        transform.LookAt(target);

        if (Input.GetKeyDown(KeyCode.C))
        {
            string filename = name + " (" + i.ToString() + ").png";
            ScreenCapture.CaptureScreenshot(filename);
            Debug.Log("Captured: " + filename);
            i++;
        }
    }
}
