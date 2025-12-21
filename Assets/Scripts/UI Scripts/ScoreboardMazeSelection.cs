using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardMazeSelection : MonoBehaviour
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
        set => controller = value;
    }

    [SerializeField] GameObject[] MazeScoringPanels;
    [SerializeField] Image[] ButtonsBack;
    [SerializeField] Color HovorColor;
    [SerializeField] GameObject ReturnScreen;

    int CurrentButton = 0;
    bool isController = false;
    CanvasScript canvasScript;

    private void Awake()
    {
        canvasScript = FindObjectOfType<CanvasScript>();
        Controller.Gamepad.Movement.started += Rotation_started;
        Controller.Gamepad.ButtonRight.canceled += ButtonRight_canceled;
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

    private void ButtonRight_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (isController)
        {
            SelectScoringMaze(CurrentButton);
        }
    }

    public void SelectScoringMaze(int mazeNumber)
    {
        canvasScript.audioManager.Play("select");
        this.gameObject.SetActive(false);
        MazeScoringPanels[mazeNumber].SetActive(true);
    }

    private void Rotation_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (isController)
        {
            Vector2 value = obj.ReadValue<Vector2>();
            if (value.y > 0) // UP
            {
                if (CurrentButton > 0)
                {
                    ButtonsBack[CurrentButton].color = Color.black;
                    CurrentButton--;
                    ButtonsBack[CurrentButton].color = HovorColor;
                }
            }
            else if (value.y < 0) // DOWN
            {
                if (CurrentButton < ButtonsBack.Length - 1)
                {
                    ButtonsBack[CurrentButton].color = Color.black;
                    CurrentButton++;
                    ButtonsBack[CurrentButton].color = HovorColor;
                }
            }
        }
    }

    public void ClickBackButton()
    {
        canvasScript.audioManager.Play("back");
        gameObject.SetActive(false);
        ReturnScreen.SetActive(true);
    }

    public void OpenInfoPanel()
    {
        canvasScript.OpenPanel(3);
    }

    private void OnEnable()
    {
        string mode = PlayerPrefs.GetString("Mode", "Touch");
        if (mode == "Touch")
        {
            isController = false;
        }
        else if (mode == "Controller")
        {
            isController = true;
        }
        else if (mode == "Holographic")
        {
            isController = true;
        }
        if (isController)
        {
            Controller.Enable();
            ButtonsBack[CurrentButton].color = HovorColor;
        }
        else
            ButtonsBack[CurrentButton].color = Color.black;
    }
    private void OnDisable()
    {
        if (isController)
            Controller.Disable();
    }
}
