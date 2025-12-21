using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ModeScreen : MonoBehaviour
{
    // FIX: Shared Controller instance
    private Controllers controllerInstance;
    Controllers Controller
    {
        get
        {
            if (controllerInstance == null)
                controllerInstance = new Controllers();
            return controllerInstance;
        }
    }

    [SerializeField] Image[] ButtonsBack;
    [SerializeField] Image[] Buttons;
    [SerializeField] Color HovorColor;
    [SerializeField] Color SelectionColor;
    [SerializeField] GameObject BackScreen;

    int CurrentButton = 0;
    bool isController = false;
    CanvasScript canvasScript;

    private void Awake()
    {
        Controller.Gamepad.Movement.started += Rotation_started;
        Controller.Gamepad.ButtonRight.canceled += ButtonRight_canceled;
        Controller.Gamepad.ButtonLeft.canceled += ButtonLeft_canceled;
        Controller.Gamepad.ButtonDown.canceled += ButtonDown_canceled;

        canvasScript = FindObjectOfType<CanvasScript>();
    }

    private void OnEnable()
    {
        string mode = PlayerPrefs.GetString("Mode", "Touch");

        if (mode == "Touch")
        {
            isController = false;
            DeselectAllAndSelectOne(0);
        }
        else if (mode == "Controller")
        {
            isController = true;
            DeselectAllAndSelectOne(1);
        }
        else if (mode == "Holographic")
        {
            isController = true;
            DeselectAllAndSelectOne(2);
        }

        // Subscribe to device change
        InputSystem.onDeviceChange += OnDeviceChange;

        if (isController)
        {
            Controller.Enable();
            ButtonsBack[CurrentButton].color = HovorColor;
        }
        else
        {
            ButtonsBack[CurrentButton].color = Color.black;
        }
    }

    private void OnDisable()
    {
        if (isController)
            Controller.Disable();

        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        Debug.Log($"Device change: {device.displayName}, Change: {change}");

        if (device is Gamepad)
        {
            if (change == InputDeviceChange.Added)
                Debug.Log("Gamepad connected: " + device.displayName);
            else if (change == InputDeviceChange.Removed)
                Debug.Log("Gamepad disconnected: " + device.displayName);
        }
        else if (device is Joystick)
        {
            if (change == InputDeviceChange.Added)
                Debug.Log("Joystick connected: " + device.displayName);
            else if (change == InputDeviceChange.Removed)
                Debug.Log("Joystick disconnected: " + device.displayName);
        }
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
                    ClickTouchButton(0);
                    break;
                case 1:
                    ClickBluetoothController(1);
                    break;
                case 2:
                    ClickHolographic(2);
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

    public void ClickTouchButton(int index)
    {
        canvasScript.audioManager.Play("select");
        PlayerPrefs.SetString("Mode", "Touch");
        ButtonsBack[CurrentButton].color = Color.black;
        canvasScript.ApplyRotations(new Vector3(0f, 0f, 0f));
        isController = false;
        Controller.Disable();
        DeselectAllAndSelectOne(index);
    }

    public void ClickBluetoothController(int index)
    {
        canvasScript.audioManager.Play("select");

        if (!isAnyControllerConnected())
        {
            Debug.LogWarning("No gamepad or joystick connected! Please pair a controller.");
            return;
        }

        PlayerPrefs.SetString("Mode", "Controller");
        ButtonsBack[CurrentButton].color = HovorColor;
        canvasScript.ApplyRotations(new Vector3(0f, 0f, 0f));
        isController = true;
        Controller.Enable();
        DeselectAllAndSelectOne(index);
    }

    public void ClickHolographic(int index)
    {
        canvasScript.audioManager.Play("select");
        PlayerPrefs.SetString("Mode", "Holographic");
        ButtonsBack[CurrentButton].color = HovorColor;
        canvasScript.ApplyRotations(new Vector3(0f, -180f, 0f));
        isController = true;
        Controller.Enable();
        DeselectAllAndSelectOne(index);
    }

    public void ClickBackButton()
    {
        canvasScript.audioManager.Play("back");
        gameObject.SetActive(false);
        BackScreen.SetActive(true);
    }

    public void OpenInfoPanel()
    {
        canvasScript.OpenPanel(1);
    }

    void DeselectAllAndSelectOne(int selectedIndex)
    {
        foreach (var item in Buttons)
            item.color = Color.white;

        Buttons[selectedIndex].color = SelectionColor;
    }

    bool isAnyControllerConnected()
    {
        if (Gamepad.all.Count > 0)
            return true;
        if (Joystick.all.Count > 0)
            return true;

        return false;
    }
}
