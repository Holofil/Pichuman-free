using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MazeScreen : MonoBehaviour
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
        set
        {
            controller = value;
        }
    }

    [SerializeField] Image[] ButtonsBack;
    [SerializeField] Color HovorColor;
    [SerializeField] GameObject BackPanel;

    int CurrentButton = 0;
    bool isController = false;

    private void Awake()
    {
        Controller.Gamepad.Movement.started += Rotation_started;
        Controller.Gamepad.ButtonRight.canceled += ButtonRight_canceled;
        Controller.Gamepad.ButtonLeft.canceled += ButtonLeft_canceled;
        //Controller.Gamepad.ButtonDown.canceled += ButtonDown_canceled;
    }

    private void ButtonLeft_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (isController)
            ClickBackButton();
    }

    private void Rotation_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (isController)
        {
            Vector2 value = obj.ReadValue<Vector2>();
            if (value.y > 0)
            {
                if (CurrentButton > 0)
                {
                    ButtonsBack[CurrentButton].color = Color.black;
                    CurrentButton--;
                    ButtonsBack[CurrentButton].color = HovorColor;
                }
            }
            else if (value.y < 0)
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

    /*private void ButtonDown_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (isController)
            Debug.Log("Open Info Panel");
    }*/

    private void ButtonRight_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (isController)
            ClickMazeSelectionButton(CurrentButton);
    }

    public void ClickMazeSelectionButton(int buttonNumber)
    {
        PlayerPrefs.SetInt("MazeNumber", buttonNumber);
        //SceneManager.LoadScene("game scene");
        StartCoroutine(LoadAsyncScene());
    }

    IEnumerator LoadAsyncScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("game scene");
        while (!asyncLoad.isDone)
        {
            Debug.Log(asyncLoad.progress + " %");
            yield return null;
        }
    }

    public void ClickBackButton()
    {
        gameObject.SetActive(false);
        BackPanel.SetActive(true);
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

    // MazeScreen.cs
    private void OnDisable()
    {
        // Unsubscribe ONLY what was subscribed in Awake
        if (Controller != null)
        {
            Controller.Gamepad.Movement.started -= Rotation_started;
            Controller.Gamepad.ButtonRight.canceled -= ButtonRight_canceled;
            Controller.Gamepad.ButtonLeft.canceled -= ButtonLeft_canceled;
        }

        if (isController)
            Controller.Disable();
    }

}
