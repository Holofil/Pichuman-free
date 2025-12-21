using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingScreen : MonoBehaviour
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
    [SerializeField] Image[] ButtonsBack;
    [SerializeField] Image[] Buttons;
    [SerializeField] TextMeshProUGUI[] ButtonText;
    [SerializeField] Color HovorColor;
    [SerializeField] Color DisableColor;
    [SerializeField] GameObject ReturnScreen;

    CanvasScript canvasScript;
    int CurrentButton = 0;
    bool isController = false;

    string MusicStatus = "ON";
    string GraphicsStatus = "ON";


    private void Awake()
    {
        Controller.Gamepad.Movement.started += Rotation_started;
        Controller.Gamepad.ButtonRight.canceled += ButtonRight_canceled;
        Controller.Gamepad.ButtonLeft.canceled += ButtonLeft_canceled;
        Controller.Gamepad.ButtonDown.canceled += ButtonDown_canceled;

        canvasScript = FindObjectOfType<CanvasScript>();
    }

    private void ButtonDown_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (isController)
            OpenInfoPanel();
    }

    private void ButtonLeft_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        ClickBackButton();
    }

    private void ButtonRight_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (isController)
        {
            switch (CurrentButton)
            {
                case 0:
                    OnClickMusic(0);
                    break;
                case 1:
                    OnClickGraphics(1);
                    break;
            }
        }
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

    public void OnClickMusic(int index)
    {
        canvasScript.audioManager.Play("toggle");
        if (MusicStatus == "ON")
        {
            canvasScript.audioManager.Stop("theme");
            canvasScript.audioManager.Play("select");
            canvasScript.audioManager.SetMusicStatus(false);
            PlayerPrefs.SetString("MusicStatus", "OFF");
            Buttons[index].color = DisableColor;
            ButtonText[index].color = Color.white;
        }
        else if (MusicStatus == "OFF")
        {
            canvasScript.audioManager.SetMusicStatus(true);
            canvasScript.audioManager.Play("theme");
            PlayerPrefs.SetString("MusicStatus", "ON");
            Buttons[index].color = Color.white;
            ButtonText[index].color = Color.black;
        }
        MusicStatus = PlayerPrefs.GetString("MusicStatus", "ON");
    }

    public void OnClickGraphics(int index)
    {
        canvasScript.audioManager.Play("toggle");
        if (GraphicsStatus == "ON")
        {
            PlayerPrefs.SetString("GraphicsStatus", "OFF");
            Buttons[index].color = DisableColor;
            ButtonText[index].color = Color.white;
        }
        else if (GraphicsStatus == "OFF")
        {
            PlayerPrefs.SetString("GraphicsStatus", "ON");
            Buttons[index].color = Color.white;
            ButtonText[index].color = Color.black;
        }
        GraphicsStatus = PlayerPrefs.GetString("GraphicsStatus", "ON");
    }

    public void ClickBackButton()
    {
        canvasScript.audioManager.Play("back");
        gameObject.SetActive(false);
        ReturnScreen.SetActive(true);
    }

    public void OpenInfoPanel()
    {
        canvasScript.OpenPanel(2);
    }

    private void OnEnable()
    {
        MusicStatus = PlayerPrefs.GetString("MusicStatus", "ON");
        if (MusicStatus == "ON")
        {
            Buttons[0].color = Color.white;
            ButtonText[0].color = Color.black;
        }
        else if (MusicStatus == "OFF")
        {
            Buttons[0].color = DisableColor;
            ButtonText[0].color = Color.white;
        }

        GraphicsStatus = PlayerPrefs.GetString("GraphicsStatus", "ON");
        if (GraphicsStatus == "ON")
        {
            Buttons[1].color = Color.white;
            ButtonText[1].color = Color.black;
        }
        else if (GraphicsStatus == "OFF")
        {
            Buttons[1].color = DisableColor;
            ButtonText[1].color = Color.white;
        }

        string mode = PlayerPrefs.GetString("Mode", "Touch");
        if (mode == "Touch")
            isController = false;
        else if (mode == "Controller" || mode == "Holographic")
            isController = true;
        else
            isController = false;

        if (isController)
        {
            Controller.Enable();
            ButtonsBack[CurrentButton].color = HovorColor;
        }
        else
            ButtonsBack[CurrentButton].color = Color.black;
    }
// Template for: ScoreboardMazeSelection.cs, SettingScreen.cs, ModeScreen.cs, MainScreen.cs
    private void OnDisable()
    {
        // Unsubscribe ALL input events
        if (Controller != null)
        {
            Controller.Gamepad.Movement.started -= Rotation_started;
            Controller.Gamepad.ButtonRight.canceled -= ButtonRight_canceled;
            Controller.Gamepad.ButtonLeft.canceled -= ButtonLeft_canceled;
            Controller.Gamepad.ButtonDown.canceled -= ButtonDown_canceled;
        }
        
        // Add script-specific unsubscriptions (e.g., ModeScreen's InputSystem.onDeviceChange)
        
        if (isController)
            Controller.Disable();
    }
}
