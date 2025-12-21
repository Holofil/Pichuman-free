using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelScreen : MonoBehaviour
{
    // FIX: Use a single shared instance like ModeScreen does
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
    
    [SerializeField] Color DeselectHovorColor;
    [SerializeField] Color DeselectFillerHovorColor;
    [SerializeField] GameObject ReturnScreenPenel;
    [SerializeField] GameObject[] SliderObjects;
    Slider[] sliders;

    int CurrentSlider = 0;
    bool isController = false;
    bool isHolographic = false;
    CanvasScript canvasScript;
    
    // Track if events are subscribed
    private bool eventsSubscribed = false;

    private void Awake()
    {
        Debug.Log("🎮 LevelScreen Awake started");
        canvasScript = FindObjectOfType<CanvasScript>();
        
        if (canvasScript == null)
        {
            Debug.LogWarning("⚠️ CanvasScript not found in scene! Audio and panel functionality may be limited.");
        }

        if (SliderObjects == null || SliderObjects.Length == 0)
        {
            Debug.LogError("🚨 SliderObjects array is null or empty! Assign sliders in the Inspector.");
            sliders = new Slider[0];
            return;
        }

        Debug.Log($"📊 Initializing {SliderObjects.Length} sliders");
        sliders = new Slider[SliderObjects.Length];
        
        for (int i = 0; i < SliderObjects.Length; i++)
        {
            if (SliderObjects[i] == null)
            {
                Debug.LogError($"🚨 SliderObjects[{i}] is null in Inspector!");
                continue;
            }
            
            sliders[i] = SliderObjects[i].GetComponent<Slider>();
            if (sliders[i] == null)
            {
                Debug.LogError($"🚨 Slider component not found on SliderObjects[{i}]!");
                continue;
            }
            
            Debug.Log($"📊 Slider {i} initialized: {sliders[i].name}");
            
            int index = i;
            sliders[i].onValueChanged.AddListener((float value) => {
                Debug.Log($"📊 Slider {index} changed to {value}");
                UpdateSliderValue(index);
            });
        }
        
        Debug.Log("✅ LevelScreen Awake completed");
    }
    
    private void Start()
    {
        SetSelectedValues();
    }

    // FIX: Separate method to subscribe to events
    private void SubscribeToControllerEvents()
    {
        if (eventsSubscribed) return;
        
        Controller.Gamepad.Movement.started += Rotation_started;
        Controller.Gamepad.ButtonLeft.canceled += ButtonLeft_canceled;
        Controller.Gamepad.ButtonDown.canceled += ButtonDown_canceled;
        
        eventsSubscribed = true;
        Debug.Log("🎮 Controller events SUBSCRIBED");
    }
    
    // FIX: Separate method to unsubscribe from events
    private void UnsubscribeFromControllerEvents()
    {
        if (!eventsSubscribed) return;
        
        Controller.Gamepad.Movement.started -= Rotation_started;
        Controller.Gamepad.ButtonLeft.canceled -= ButtonLeft_canceled;
        Controller.Gamepad.ButtonDown.canceled -= ButtonDown_canceled;
        
        eventsSubscribed = false;
        Debug.Log("🎮 Controller events UNSUBSCRIBED");
    }

    private void ButtonDown_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Debug.Log("🎮 ButtonDown pressed, isController: " + isController);
        if (isController)
            OpenInfoPanel();
    }

    private void ButtonLeft_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Debug.Log("🎮 ButtonLeft pressed");
        ClickBackButton();
    }

    private void Rotation_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Debug.Log("🎮 Rotation input detected, isController: " + isController);
        
        if (!isController) return;

        Vector2 value = obj.ReadValue<Vector2>();
        Debug.Log($"🎮 Input Vector: {value}");
        
        if (value.y > 0)
        {
            if (CurrentSlider > 0)
            {
                DeselectSliderColors(CurrentSlider);
                CurrentSlider--;
                SelectSliderColors(CurrentSlider);
                PlaySelectSound();
                Debug.Log($"🎮 Moved UP to slider {CurrentSlider}");
            }
        }
        else if (value.y < 0)
        {
            if (CurrentSlider < SliderObjects.Length - 1)
            {
                DeselectSliderColors(CurrentSlider);
                CurrentSlider++;
                SelectSliderColors(CurrentSlider);
                PlaySelectSound();
                Debug.Log($"🎮 Moved DOWN to slider {CurrentSlider}");
            }
        }

        // 🔄 Handle horizontal input (mirror in holographic mode)
        float xInput = isHolographic ? -value.x : value.x;

        if (xInput < 0)
        {
            if (sliders[CurrentSlider].value > sliders[CurrentSlider].minValue)
            {
                PlaySelectSound();
                sliders[CurrentSlider].value -= 1;
                UpdateSliderValue(CurrentSlider);
                Debug.Log($"🎮 {(isHolographic ? "Mirrored " : "")}Decreased slider {CurrentSlider} to {sliders[CurrentSlider].value}");
            }
        }
        else if (xInput > 0)
        {
            if (sliders[CurrentSlider].value < sliders[CurrentSlider].maxValue)
            {
                PlaySelectSound();
                sliders[CurrentSlider].value += 1;
                UpdateSliderValue(CurrentSlider);
                Debug.Log($"🎮 {(isHolographic ? "Mirrored " : "")}Increased slider {CurrentSlider} to {sliders[CurrentSlider].value}");
            }
        }

    }
    
    private void PlaySelectSound()
    {
        if (canvasScript != null && canvasScript.audioManager != null)
        {
            canvasScript.audioManager.Play("select");
        }
    }

    void UpdateSliderValue(int sliderIndex)
    {
        switch (sliderIndex)
        {
            case 0:
                ChangeMazeNumberSlider();
                break;
            case 1:
                ChangeGhostNumberSlider();
                break;
            case 2:
                ChangeGhostSpeedSlider();
                break;
        }
    }

    void DeselectSliderColors(int index)
    {
        if (index < 0 || index >= SliderObjects.Length || SliderObjects[index] == null) return;

        Transform backgroundTransform = SliderObjects[index].transform.Find("Background");
        if (backgroundTransform != null)
        {
            Image backgroundImage = backgroundTransform.GetComponent<Image>();
            if (backgroundImage != null)
                backgroundImage.color = DeselectHovorColor;
        }

        Transform fillAreaTransform = SliderObjects[index].transform.Find("Fill Area/Fill");
        if (fillAreaTransform != null)
        {
            Image fillAreaImage = fillAreaTransform.GetComponent<Image>();
            if (fillAreaImage != null)
                fillAreaImage.color = DeselectFillerHovorColor;
        }

        Transform handleTransform = SliderObjects[index].transform.Find("Handle Slide Area/Handle");
        if (handleTransform != null)
        {
            Image handleImage = handleTransform.GetComponent<Image>();
            if (handleImage != null)
                handleImage.color = DeselectHovorColor;
        }
    }

    void LoadSelectedMaze(int mazeIndex)
    {
        Debug.Log($"🔄 Selected Maze{mazeIndex + 1} (index: {mazeIndex}). Will activate in game scene.");
    }

    void SelectSliderColors(int index)
    {
        if (index < 0 || index >= SliderObjects.Length || SliderObjects[index] == null) return;

        Transform backgroundTransform = SliderObjects[index].transform.Find("Background");
        if (backgroundTransform != null)
        {
            Image backgroundImage = backgroundTransform.GetComponent<Image>();
            if (backgroundImage != null)
                backgroundImage.color = Color.white;
        }

        Transform fillAreaTransform = SliderObjects[index].transform.Find("Fill Area/Fill");
        if (fillAreaTransform != null)
        {
            Image fillAreaImage = fillAreaTransform.GetComponent<Image>();
            if (fillAreaImage != null)
                fillAreaImage.color = Color.red;
        }

        Transform handleTransform = SliderObjects[index].transform.Find("Handle Slide Area/Handle");
        if (handleTransform != null)
        {
            Image handleImage = handleTransform.GetComponent<Image>();
            if (handleImage != null)
                handleImage.color = Color.white;
        }
    }

    public void ChangeMazeNumberSlider()
    {
        if (sliders == null || sliders.Length == 0 || sliders[0] == null)
        {
            Debug.LogError("🚨 Maze slider is null! Check SliderObjects[0] in the Inspector.");
            return;
        }

        int mazeIndex = Mathf.RoundToInt(sliders[0].value - 1);
        Debug.Log($"🟢 Slider value: {sliders[0].value}, Maze index: {mazeIndex} (Maze{mazeIndex + 1})");

        PlayerPrefs.SetInt("MazeNumber", mazeIndex);
        PlayerPrefs.Save();

        Transform valueTransform = SliderObjects[0].transform.Find("value");
        if (valueTransform != null)
        {
            TextMeshProUGUI textValueObj = valueTransform.GetComponent<TextMeshProUGUI>();
            if (textValueObj != null)
            {
                textValueObj.text = sliders[0].value.ToString();
            }
        }

        LoadSelectedMaze(mazeIndex);
    }

    public void ChangeGhostNumberSlider()
    {
        if (sliders == null || sliders.Length <= 1 || sliders[1] == null)
        {
            Debug.LogError("🚨 Ghost number slider is null! Check SliderObjects[1] in the Inspector.");
            return;
        }

        int ghostCount = Mathf.RoundToInt(sliders[1].value);
        PlayerPrefs.SetInt("NumberOfGhosts", ghostCount);
        PlayerPrefs.Save();

        Transform valueTransform = SliderObjects[1].transform.Find("value");
        if (valueTransform != null)
        {
            TextMeshProUGUI textValueObj = valueTransform.GetComponent<TextMeshProUGUI>();
            if (textValueObj != null)
            {
                textValueObj.text = ghostCount.ToString();
            }
        }

        Debug.Log($"👻 Number of Ghosts set to {ghostCount} and saved.");
    }

    public void ChangeGhostSpeedSlider()
    {
        if (sliders == null || sliders.Length <= 2 || sliders[2] == null)
        {
            Debug.LogError("🚨 Ghost speed slider is null! Check SliderObjects[2] in the Inspector.");
            return;
        }

        int speedValue = Mathf.RoundToInt(sliders[2].value);
        PlayerPrefs.SetInt("GhostsSpeed", speedValue);
        PlayerPrefs.Save();

        Transform valueTransform = SliderObjects[2].transform.Find("value");
        if (valueTransform != null)
        {
            TextMeshProUGUI textValue = valueTransform.GetComponent<TextMeshProUGUI>();
            if (textValue != null)
            {
                switch (speedValue)
                {
                    case 1: textValue.text = "Slow"; break;
                    case 2: textValue.text = "Fast"; break;
                    case 3: textValue.text = "Faster"; break;
                    default: textValue.text = "Unknown"; break;
                }
            }
        }

        Debug.Log($"⚡ Ghost Speed set to {speedValue} and saved.");
    }

    public void ClickBackButton()
    {
        if (canvasScript != null && canvasScript.audioManager != null)
        {
            canvasScript.audioManager.Play("back");
        }
        
        PlayerPrefs.Save();
        Debug.Log("ℹ️ PlayerPrefs saved before returning to previous screen.");
        
        gameObject.SetActive(false);
        if (ReturnScreenPenel != null)
        {
            ReturnScreenPenel.SetActive(true);
        }
        else
        {
            Debug.LogError("🚨 ReturnScreenPenel is null! Cannot return to previous screen.");
        }
    }

    public void StartGame()
    {
        PlayerPrefs.Save();
        Debug.Log("ℹ️ PlayerPrefs saved before loading game scene.");
        StartCoroutine(LoadAsyncScene());
    }

    IEnumerator LoadAsyncScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("game scene");
        while (!asyncLoad.isDone)
        {
            Debug.Log($"🌍 Loading game scene: {asyncLoad.progress * 100}%");
            yield return null;
        }
    }

    void SetSelectedValues()
    {
        int mazeNum = PlayerPrefs.GetInt("MazeNumber", 0);
        int ghostNum = PlayerPrefs.GetInt("NumberOfGhosts", 3);
        int ghostSpeed = PlayerPrefs.GetInt("GhostsSpeed", 1);

        Debug.Log($"🔄 Loading initial settings - Maze: {mazeNum} (Maze{mazeNum + 1}), Ghosts: {ghostNum}, Speed: {ghostSpeed}");

        if (sliders == null || sliders.Length < 3)
        {
            Debug.LogError("🚨 Sliders array not initialized or too small! Check SliderObjects in the Inspector.");
            return;
        }

        if (sliders[0] != null)
        {
            sliders[0].onValueChanged.RemoveAllListeners();
            sliders[0].value = mazeNum + 1;
            UpdateSliderText(0, (mazeNum + 1).ToString());
            int index = 0;
            sliders[0].onValueChanged.AddListener((float value) => {
                UpdateSliderValue(index);
            });
        }

        if (sliders[1] != null)
        {
            sliders[1].onValueChanged.RemoveAllListeners();
            sliders[1].value = ghostNum;
            UpdateSliderText(1, ghostNum.ToString());
            int index = 1;
            sliders[1].onValueChanged.AddListener((float value) => {
                UpdateSliderValue(index);
            });
        }

        if (sliders[2] != null)
        {
            sliders[2].onValueChanged.RemoveAllListeners();
            sliders[2].value = ghostSpeed;
            string speedText = ghostSpeed == 1 ? "Slow" : (ghostSpeed == 2 ? "Fast" : "Faster");
            UpdateSliderText(2, speedText);
            int index = 2;
            sliders[2].onValueChanged.AddListener((float value) => {
                UpdateSliderValue(index);
            });
        }
        
        PlayerPrefs.SetInt("MazeNumber", mazeNum);
        PlayerPrefs.SetInt("NumberOfGhosts", ghostNum);
        PlayerPrefs.SetInt("GhostsSpeed", ghostSpeed);
        PlayerPrefs.Save();
        
        Debug.Log("📋 All initial values set, now loading maze");
        LoadSelectedMaze(mazeNum);
    }
    
    private void UpdateSliderText(int sliderIndex, string text)
    {
        if (sliderIndex < 0 || sliderIndex >= SliderObjects.Length || SliderObjects[sliderIndex] == null) return;
        
        Transform valueTransform = SliderObjects[sliderIndex].transform.Find("value");
        if (valueTransform != null)
        {
            TextMeshProUGUI textValue = valueTransform.GetComponent<TextMeshProUGUI>();
            if (textValue != null)
            {
                textValue.text = text;
            }
        }
    }

    public void OpenInfoPanel()
    {
        if (canvasScript != null)
        {
            canvasScript.OpenPanel(4);
        }
        else
        {
            Debug.LogWarning("⚠️ canvasScript is null! Cannot open info panel.");
        }
    }

    private void OnEnable()
    {
        Debug.Log("🎮 LevelScreen OnEnable called");
        
        CurrentSlider = 0;
        
        string mode = PlayerPrefs.GetString("Mode", "Touch");
        Debug.Log($"🎮 Current Mode: {mode}");
        
        if (mode == "Touch")
        {
            isController = false;
            isHolographic = false;
        }
        else if (mode == "Controller")
        {
            isController = true;
            isHolographic = false;
        }
        else if (mode == "Holographic")
        {
            isController = true;
            isHolographic = true;
        }
        
        // FIX: Always subscribe to events when screen is enabled
        SubscribeToControllerEvents();
        
        // FIX: Enable controller when needed
        if (isController)
        {
            Controller.Enable();
            Debug.Log("🎮 Controller ENABLED in LevelScreen");
        }
        
        if (sliders != null)
        {
            for (int i = 0; i < SliderObjects.Length; i++)
            {
                if (i < sliders.Length && sliders[i] != null)
                {
                    sliders[i].interactable = !isController;
                    if (isController)
                    {
                        DeselectSliderColors(i);
                    }
                    else
                    {
                        SelectSliderColors(i);
                    }
                }
            }
        }
        
        SetSelectedValues();
        
        if (isController && CurrentSlider < SliderObjects.Length)
        {
            SelectSliderColors(CurrentSlider);
            Debug.Log($"🎮 Selected slider {CurrentSlider}");
        }
    }

    private void OnDisable()
    {
        Debug.Log("🎮 LevelScreen OnDisable called");
        
        // FIX: Always unsubscribe when screen is disabled
        UnsubscribeFromControllerEvents();

        if (isController && Controller != null)
        {
            Controller.Disable();
            Debug.Log("🎮 Controller DISABLED in LevelScreen");
        }
    }
}