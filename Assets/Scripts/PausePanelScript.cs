using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PausePanelScript : MonoBehaviour
{
    // 🔥 UNIQUE Controller instance - NOT shared with other scripts!
    private Controllers pauseController;

    int Current = 0;
    bool isController = false;
    GameManager gameManager;

    [Header("Pause Panel Elements")]
    [SerializeField] GameObject[] ButtonsBack;
    [SerializeField] Color HoverColor = Color.red;
    [SerializeField] Color NormalColor = Color.black;

    // 🔒 Lock to prevent action while already processing
    private bool isProcessingAction = false;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    // Called manually from GameManager when pause opens
    public void OnPanelActivated()
    {
        string mode = PlayerPrefs.GetString("Mode", "Touch");
        isController = (mode == "Controller" || mode == "Holographic");

        if (isController)
        {
            // 🔥 Create a FRESH controller instance just for pause menu
            if (pauseController != null)
            {
                pauseController.Dispose(); // Clean up old instance
            }
            pauseController = new Controllers();

            // Reset state
            isProcessingAction = false;
            
            SetupButtons();

            // Subscribe to THIS pause controller only
            pauseController.Gamepad.Movement.performed += Rotation_performed;
            pauseController.Gamepad.ButtonRight.started += ButtonRight_started;

            // Enable this controller
            pauseController.Enable();
            
            Debug.Log("🎮 PausePanelScript: Created ISOLATED controller instance");
        }
    }

    private void OnDisable()
    {
        Debug.Log("🎮 PausePanelScript: OnDisable called");
        
        if (isController && pauseController != null)
        {
            // Unsubscribe from our isolated controller
            pauseController.Gamepad.Movement.performed -= Rotation_performed;
            pauseController.Gamepad.ButtonRight.started -= ButtonRight_started;
            
            // Disable and dispose our controller
            pauseController.Disable();
            pauseController.Dispose();
            pauseController = null;
            
            Debug.Log("🎮 PausePanelScript: Disposed isolated controller");
        }
        
        // Reset lock
        isProcessingAction = false;
    }

    private void Rotation_performed(InputAction.CallbackContext obj)
    {
        if (!isController) return;
        if (isProcessingAction) return; // Don't navigate while processing action

        Vector2 val = obj.ReadValue<Vector2>();
        if (val.y < 0 && Current < ButtonsBack.Length - 1)
        {
            ButtonsBack[Current].GetComponent<Image>().color = NormalColor;
            Current++;
            ButtonsBack[Current].GetComponent<Image>().color = HoverColor;
        }
        else if (val.y > 0 && Current > 0)
        {
            ButtonsBack[Current].GetComponent<Image>().color = NormalColor;
            Current--;
            ButtonsBack[Current].GetComponent<Image>().color = HoverColor;
        }
    }

    private void ButtonRight_started(InputAction.CallbackContext obj)
    {
        if (!isController) return;
        
        // 🔒 Prevent double-trigger
        if (isProcessingAction)
        {
            Debug.Log("⏸️ Button press ignored - already processing");
            return;
        }

        isProcessingAction = true;
        Debug.Log($"✅ ButtonRight pressed - Selection: {Current}");

        // Immediately unsubscribe to prevent any chance of re-trigger
        if (pauseController != null)
        {
            pauseController.Gamepad.ButtonRight.started -= ButtonRight_started;
            pauseController.Gamepad.Movement.performed -= Rotation_performed;
        }

        // Execute action based on current selection
        if (Current == 0)
        {
            Debug.Log("▶️ RESUME selected");
            StartCoroutine(ExecuteResume());
        }
        else if (Current == 1)
        {
            Debug.Log("🚪 EXIT selected");
            StartCoroutine(ExecuteExit());
        }
    }

    // Coroutine to handle Resume with proper cleanup
    private IEnumerator ExecuteResume()
    {
        // Small delay to ensure input is fully released
        yield return new WaitForSecondsRealtime(0.15f);
        
        // Disable and dispose controller BEFORE calling resume
        if (pauseController != null)
        {
            pauseController.Disable();
            pauseController.Dispose();
            pauseController = null;
            Debug.Log("🎮 Disposed controller before Resume");
        }
        
        // Small additional delay
        yield return new WaitForSecondsRealtime(0.1f);
        
        // Now call resume
        if (gameManager != null)
        {
            gameManager.OnClickResume();
        }
    }

    // Coroutine to handle Exit with proper cleanup
    private IEnumerator ExecuteExit()
    {
        // Small delay to ensure input is fully released
        yield return new WaitForSecondsRealtime(0.15f);
        
        // Disable and dispose controller BEFORE calling exit
        if (pauseController != null)
        {
            pauseController.Disable();
            pauseController.Dispose();
            pauseController = null;
            Debug.Log("🎮 Disposed controller before Exit");
        }
        
        // Small additional delay
        yield return new WaitForSecondsRealtime(0.1f);
        
        // Now call exit
        if (gameManager != null)
        {
            gameManager.OnClickExit();
        }
    }

    void SetupButtons()
    {
        Current = 0;
        foreach (var item in ButtonsBack)
            item.GetComponent<Image>().color = NormalColor;

        if (ButtonsBack.Length > 0)
            ButtonsBack[Current].GetComponent<Image>().color = HoverColor;
    }
}