using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasScript : MonoBehaviour
{
    [SerializeField] Transform[] RotatedGameObjects;
    [SerializeField] GameObject[] Panels;
    [SerializeField] GameObject InfoPanel;
    [SerializeField] GameObject TeamsAndConditionPanel;

    int ReturningPanel;
    public AudioManager audioManager { get; private set; }

    private void Awake()
    {
        audioManager = FindObjectOfType<AudioManager>();

        int a = PlayerPrefs.GetInt("Agree", -1);
        if (a == -1)
        {
            TeamsAndConditionPanel.SetActive(true);
        }
        else
        {
            TeamsAndConditionPanel.SetActive(false);
        }
    }


    public void OnClickAgree()
    {
        audioManager.Play("select");
        PlayerPrefs.SetInt("Agree", 1);
        TeamsAndConditionPanel.SetActive(false);
    }

    public void OnClickWebURL()
    {
        Application.OpenURL("https://www.holofil.com");
    }

    public void OnClickTermAndConditions()
    {
        Application.OpenURL("https://www.holofil.com/terms-and-conditions");
    }

    public void OnClickPrivacyPolicy()
    {
        Application.OpenURL("https://www.holofil.com/privacy-policy");
    }



    private void Start()
    {
        string mode = PlayerPrefs.GetString("Mode", "Touch");
        if (mode == "Touch" || mode == "Controller")
        {
            ApplyRotations(new Vector3(0f, 0f, 0f));
        }
        else if (mode == "Holographic")
        {
            ApplyRotations(new Vector3(0f, -180f, 0f));
        }
    }

    public void CloseInfoScreenAndReturn()
    {
        audioManager.Play("back");
        InfoPanel.SetActive(false);
        Panels[ReturningPanel].SetActive(true);
    }

    public void OpenPanel(int _panelNumber)
    {
        audioManager.Play("select");
        Panels[_panelNumber].SetActive(false);
        ReturningPanel = _panelNumber;
        InfoPanel.SetActive(true);
    }

    public void ApplyRotations(Vector3 _Rotation)
    {
        foreach (var Rotated in RotatedGameObjects)
        {
            Rotated.rotation = Quaternion.Euler(_Rotation);
        }
    }
}
