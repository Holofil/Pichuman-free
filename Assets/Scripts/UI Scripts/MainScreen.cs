using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MainScreen : MonoBehaviour
{
    private Controllers controllerInstance;

    Controllers Controller
    {
        get
        {
            if (controllerInstance == null)
            {
                controllerInstance = new Controllers();
                controllerInstance.Enable();
                Debug.Log("✅ Controller instance created and enabled.");
            }
            return controllerInstance;
        }
    }

    [SerializeField] Image[] ButtonsBack;
    [SerializeField] GameObject[] ButtonOpeningPanel;
    [SerializeField] Color HovorColor;

    [SerializeField] GameObject RateUsPanel, AlreadyRatedPanel;
    [SerializeField] int RateUsNumber = 1;
    [SerializeField] int AlreadyRateUsNumber = 2;

    int CurrentButton = 0;
    bool isController = false;
    CanvasScript canvasScript;
    bool isSubscribed = false;

    private void Awake()
    {
        SubscribeControllerInputs();
    }

    private void Start()
    {
        canvasScript = FindObjectOfType<CanvasScript>();
        canvasScript.audioManager.Play("theme");
        RatePageManager();

        Debug.Log($"🎮 Active gamepads: {Gamepad.all.Count}");
        if (Gamepad.current != null)
        {
            Debug.Log($"🟢 Current Gamepad: {Gamepad.current.displayName}");
        }
        else
        {
            Debug.LogWarning("🔴 No active Gamepad found!");
        }
    }

    public void ClickPlayButton()
    {
        Debug.Log("🎬 ClickPlayButton: Loading 'game scene' from UI");
        if (canvasScript != null && canvasScript.audioManager != null)
        {
            canvasScript.audioManager.Stop("theme");
        }
        else
        {
            Debug.LogError("canvasScript or audioManager is null! Check assignments in the Inspector.");
        }

        Physics.SyncTransforms();
        Debug.Log("✅ Physics.SyncTransforms done before scene load");

        StartCoroutine(LoadAsyncScene());
    }

    IEnumerator LoadAsyncScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("game scene");

        if (asyncLoad == null)
        {
            Debug.LogError("Scene load failed! Ensure 'game scene' is added to Build Settings.");
            yield break;
        }

        while (!asyncLoad.isDone)
        {
            Debug.Log((asyncLoad.progress * 100) + " %");
            yield return null;
        }
    }

    private void SubscribeControllerInputs()
    {
        if (isSubscribed)
            return;

        Controller.Gamepad.Movement.started += Rotation_started;
        Controller.Gamepad.ButtonRight.canceled += ButtonRight_canceled;
        Controller.Gamepad.ButtonDown.canceled += ButtonDown_canceled;

        isSubscribed = true;
        Debug.Log("✅ Controller input events subscribed.");
    }

    private void Rotation_started(InputAction.CallbackContext obj)
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

    private void ButtonRight_canceled(InputAction.CallbackContext obj)
    {
        if (isController)
            SelectOptionMainMenu(CurrentButton);
    }

    private void ButtonDown_canceled(InputAction.CallbackContext obj)
    {
        if (isController)
            OpenInfoPanel();
    }

    public void SelectOptionMainMenu(int ButtonNumber)
    {
        canvasScript.audioManager.Play("select");
        if (ButtonNumber == ButtonsBack.Length - 1)
            ClickPlayButton();
        else
        {
            gameObject.SetActive(false);
            ButtonOpeningPanel[ButtonNumber].SetActive(true);
        }
    }

    public void OpenInfoPanel()
    {
        canvasScript.OpenPanel(0);
    }

    // MainScreen.cs
    private void OnEnable()
    {
        string mode = PlayerPrefs.GetString("Mode", "Touch");

        if (mode == "Touch")
            isController = false;
        else if (mode == "Controller")
            isController = true;
        else if (mode == "Holographic")
            isController = true;
        else
            isController = false;

        if (isController)
        {
            // 🌟 ADD THIS LINE 🌟: Explicitly enable the controller when the screen is enabled
            Controller.Enable(); 
            
            SubscribeControllerInputs(); // ensure always subscribed
            ButtonsBack[CurrentButton].color = HovorColor;
        }
        else
        {
            // 🌟 ADD THIS LINE 🌟: Explicitly disable the controller if not in controller mode
            Controller.Disable(); 
            ButtonsBack[CurrentButton].color = Color.black;
        }

        RateUsPanel.SetActive(false);
        AlreadyRatedPanel.SetActive(false);
    }

    private void OnDisable()
    {
        // ❌ IGNORE the old comment about not disabling—input events MUST be cleaned up.
        // Unsubscribe the events so this screen doesn't respond to input when disabled.
        UnsubscribeControllerInputs();
        
        // Also disable the controller if it's currently enabled, matching the logic in other UI scripts.
        if (isController && controllerInstance != null)
        {
            controllerInstance.Disable();
        }
    }
    private void UnsubscribeControllerInputs()
    {
        if (!isSubscribed)
            return;

        Controller.Gamepad.Movement.started -= Rotation_started;
        Controller.Gamepad.ButtonRight.canceled -= ButtonRight_canceled;
        Controller.Gamepad.ButtonDown.canceled -= ButtonDown_canceled;

        isSubscribed = false;
        Debug.Log("✅ Controller input events unsubscribed.");
    }
    void RatePageManager()
    {
        int NoOfGames = PlayerPrefs.GetInt("NumberOfGames", 0);
        int RateUsCheck = PlayerPrefs.GetInt("Rated", 0);

        if (NoOfGames >= RateUsNumber && RateUsCheck == 0)
            RateUsPanel.SetActive(true);
        else if (NoOfGames >= AlreadyRateUsNumber && RateUsCheck == 1)
            AlreadyRatedPanel.SetActive(true);
    }
}
