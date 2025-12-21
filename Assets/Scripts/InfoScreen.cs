using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoScreen : MonoBehaviour
{
    [SerializeField] Scrollbar bar;

    Controllers controller;
    Controllers Controller
    {
        get
        {
            if (controller == null)
                controller = new Controllers();
            return controller;
        }
        set
        {
            controller = value;
        }
    }

    bool isController = false;

    private void Awake()
    {
        Controller.Gamepad.Movement.started += Rotation_started;
        Controller.Gamepad.ButtonLeft.canceled += ButtonLeft_canceled;
    }

    private void ButtonLeft_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (isController)
            CloseInfoPanel();
    }


    private void Rotation_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (isController)
        {
            Vector2 value = obj.ReadValue<Vector2>();
            if (value.y > 0) // UP
            {
                if (bar.value < 1f)
                {
                    bar.value += 0.05f;
                    if (bar.value > 1f)
                        bar.value = 1f;
                }
            }
            else if (value.y < 0) // DOWN
            {
                if (bar.value > 0)
                {
                    bar.value -= 0.05f;
                    if (bar.value < 0)
                        bar.value = 0;
                }
            }
        }
    }

    public void CloseInfoPanel()
    {
        FindObjectOfType<CanvasScript>().CloseInfoScreenAndReturn();
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
        else
            isController = false;

        if (isController)
            Controller.Enable();
    }
    private void OnDisable()
    {
        if (isController)
            Controller.Disable();
    }
}
