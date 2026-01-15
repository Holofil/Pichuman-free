using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardMazeSelection : MonoBehaviour
{
    Controllers controller;

    [SerializeField] GameObject[] MazeScoringPanels;
    [SerializeField] Image[] ButtonsBack;
    [SerializeField] Color HovorColor;
    [SerializeField] GameObject ReturnScreen;

    int CurrentButton = 0;
    bool isController = false;
    CanvasScript canvasScript;
    bool inputsHooked = false;
    bool isSwitchingScreen = false;

    private void Awake()
    {
        canvasScript = FindObjectOfType<CanvasScript>();
        controller = new Controllers();
    }

    void HookInputs()
    {
        if (controller == null || inputsHooked)
            return;

        controller.Gamepad.Movement.started += Rotation_started;
        controller.Gamepad.ButtonRight.performed += ButtonRight_performed;
        controller.Gamepad.ButtonLeft.performed += ButtonLeft_performed;
        controller.Gamepad.ButtonDown.performed += ButtonDown_performed;
        inputsHooked = true;
    }

    void UnhookInputs()
    {
        if (controller == null || !inputsHooked)
            return;

        controller.Gamepad.Movement.started -= Rotation_started;
        controller.Gamepad.ButtonRight.performed -= ButtonRight_performed;
        controller.Gamepad.ButtonLeft.performed -= ButtonLeft_performed;
        controller.Gamepad.ButtonDown.performed -= ButtonDown_performed;
        inputsHooked = false;
    }

    IEnumerator SwitchToScreenNextFrame(GameObject nextScreen)
    {
        if (isSwitchingScreen)
            yield break;

        isSwitchingScreen = true;

        // Important for Android/InputSystem reliability: don't disable this GameObject
        // while we're still inside an input callback.
        yield return null;

        gameObject.SetActive(false);
        if (nextScreen != null)
            nextScreen.SetActive(true);

        isSwitchingScreen = false;
    }

    private void ButtonDown_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (isController)
            OpenInfoPanel();
    }

    private void ButtonLeft_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(isController)
            ClickBackButton();
    }

    private void ButtonRight_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (isController)
        {
            SelectScoringMaze(CurrentButton);
        }
    }

    public void SelectScoringMaze(int mazeNumber)
    {
        canvasScript.audioManager.Play("select");
        if (MazeScoringPanels == null || mazeNumber < 0 || mazeNumber >= MazeScoringPanels.Length)
            return;

        StartCoroutine(SwitchToScreenNextFrame(MazeScoringPanels[mazeNumber]));
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
        StartCoroutine(SwitchToScreenNextFrame(ReturnScreen));
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

        if (ButtonsBack != null && ButtonsBack.Length > 0)
        {
            for (int i = 0; i < ButtonsBack.Length; i++)
            {
                if (ButtonsBack[i] != null)
                    ButtonsBack[i].color = Color.black;
            }

            CurrentButton = Mathf.Clamp(CurrentButton, 0, ButtonsBack.Length - 1);
        }

        if (isController)
        {
            HookInputs();
            controller.Enable();
            if (ButtonsBack != null && ButtonsBack.Length > 0 && ButtonsBack[CurrentButton] != null)
                ButtonsBack[CurrentButton].color = HovorColor;
        }
        else
        {
            UnhookInputs();
            if (controller != null)
                controller.Disable();
            if (ButtonsBack != null && ButtonsBack.Length > 0 && ButtonsBack[CurrentButton] != null)
                ButtonsBack[CurrentButton].color = Color.black;
        }
    }
    private void OnDisable()
    {
        if (controller == null)
            return;

        UnhookInputs();
        controller.Disable();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        // Android builds can temporarily lose focus and leave InputActions disabled.
        if (!hasFocus)
            return;

        if (!isActiveAndEnabled || controller == null)
            return;

        if (isController)
        {
            HookInputs();
            controller.Enable();
        }
    }
}
