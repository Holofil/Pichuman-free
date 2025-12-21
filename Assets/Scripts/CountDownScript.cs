using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountDownScript : MonoBehaviour
{
    AudioManager audioManager;

     void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
    }

    public void PlaySound()
    {
        audioManager.PlayNewSound("3 2 1 go");
    }

    public void ResumeEvent()
    {
        this.gameObject.SetActive(false);
    }
}
