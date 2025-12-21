using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoTarget : MonoBehaviour {

	// Use this for initialization
	void Start () {
        foreach (var camera in (AbstractCamera[])FindObjectsOfType(typeof(AbstractCamera)))
        {
            camera.AddTarget(transform);
        }
    }
}
