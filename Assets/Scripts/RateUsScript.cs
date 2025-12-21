using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RateUsScript : MonoBehaviour
{
    Animator animator;
    AudioManager audioManager;

    [Header("URL's")]
    [SerializeField] string Android_URL;
    [SerializeField] string IOS_URL;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioManager = FindObjectOfType<AudioManager>();
    }

    private void OnEnable()
    {
        animator.Play("PanelUp");
    }

    public void ClickRateUs()
    {
        audioManager.Play("select");
        CloseRateUsPanel();
        PlayerPrefs.SetInt("Rated", 1);
#if UNITY_ANDROID
        Application.OpenURL(Android_URL);
#elif UNITY_IOS
        Application.OpenURL(IOS_URL);
#endif
    }

    public void ClickAlreadyRated()
    {
        audioManager.Play("select");
        animator.SetTrigger("CloseRateUs");
    }

    public void ClickLater()
    {
        audioManager.Play("select");
        animator.SetTrigger("CloseRateUs");
    }

    public void CloseRateUsPanel()
    {
        gameObject.SetActive(false);
        PlayerPrefs.SetInt("NumberOfGames", 0);
    }
}
