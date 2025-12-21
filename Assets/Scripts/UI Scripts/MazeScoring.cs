using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MazeScoring : MonoBehaviour
{
    Controllers controller;
    Controllers Controller
    {
        get
        {
            if (controller == null)
                controller = new Controllers();
            return controller;
        }
    }

    [SerializeField] int MazeNumber = 0;
    [SerializeField] TextMeshProUGUI[] ScoreText;
    [SerializeField] GameObject ReturnPanel;

    bool isController = false;
    CanvasScript canvasScript;

    private void Awake()
    {
        canvasScript = FindObjectOfType<CanvasScript>();
        Controller.Gamepad.ButtonLeft.canceled += ButtonLeft_canceled;
        Controller.Gamepad.ButtonDown.canceled += ButtonDown_canceled;
    }

    private void ButtonDown_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (isController)
            OpenInfoPanel();
    }

    private void ButtonLeft_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(isController)
            ClickBackButton();
    }

    public void OpenInfoPanel()
    {
        canvasScript.OpenPanel(MazeNumber + 4);
    }

    public void ClickBackButton()
    {
        canvasScript.audioManager.Play("back");
        gameObject.SetActive(false);
        ReturnPanel.SetActive(true);
    }

    void DisplayData()
    {
        int i = 0;
        for (int speedType = 1; speedType <= 3; speedType++)
        {
            for (int numberOfGhosts = 3; numberOfGhosts <= 5; numberOfGhosts++)
            {
                ScoreText[i].text = LoadData(MazeNumber, numberOfGhosts, speedType);
                i++;
            }
        }
    }

    string LoadData(int _mazeNumber, int _numberOfGhosts, int _ghostSpeedType)
    {
        string SaveTimeStr = _mazeNumber.ToString() + _numberOfGhosts.ToString() + _ghostSpeedType.ToString();
        string KeyTotalTimeInStr = SaveTimeStr + "TotalTimeStr";
        return PlayerPrefs.GetString(KeyTotalTimeInStr, "- : -");
    }

    private void OnEnable()
    {
        string mode = PlayerPrefs.GetString("Mode", "Touch");
        if (mode == "Touch")
        {
            isController = false;
        }
        else if (mode == "Controller" || mode == "Holographic")
        {
            isController = true;
        }
        if (isController)
            Controller.Enable();
        DisplayData();
    }
    // MazeScoring.cs
    private void OnDisable()
    {
        // Unsubscribe ONLY what was subscribed in Awake
        if (Controller != null)
        {
            Controller.Gamepad.ButtonLeft.canceled -= ButtonLeft_canceled;
            Controller.Gamepad.ButtonDown.canceled -= ButtonDown_canceled;
        }

        if (isController)
            Controller.Disable();
    }

}
