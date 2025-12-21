using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

/// <summary>
/// Handles ad timing using three coordinated timers:
/// 1. Global timer → Tracks total time since game start (never stops)
/// 2. Menu (UI) timer → Tracks time spent in UI scene
/// 3. Game timer → Tracks time spent in gameplay scene
/// 
/// Ad logic:
/// - If total (global) time >= 120 seconds and player returns to UI scene, show ad.
/// - Timer continues across scenes and resets only after showing an ad.
/// </summary>
public class AdTimerManager : MonoBehaviour
{
    private const float AdTriggerTime = 30f; // ⏱ 2 minutes i.e 120 seconds
    private int adsShownCount = 0;
    private UpgradePopupManager popupManager;

    private float globalTime = 0f;     // Runs constantly
    private float uiSceneTime = 0f;    // Runs only in UI scene
    private float gameSceneTime = 0f;  // Runs only in Game scene

    private Interstitial interstitialAd;
    private string currentScene;
    private bool adShownThisCycle = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        interstitialAd = FindObjectOfType<Interstitial>();
        if (interstitialAd == null)
            Debug.LogWarning("AdTimerManager: Interstitial not found. Ads won't show until detected.");

        currentScene = SceneManager.GetActiveScene().name;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentScene = scene.name;
        interstitialAd = FindObjectOfType<Interstitial>();

        // ✅ Correctly find inactive popup objects in the loaded scene
        popupManager = Resources.FindObjectsOfTypeAll<UpgradePopupManager>()
            .FirstOrDefault(m => m.gameObject.scene.isLoaded);

        // Debug confirmation   
        if (popupManager != null)
            Debug.Log("✅ Found UpgradePopupManager — ready to show popup!");
        else
            Debug.LogWarning("⚠️ Could not find UpgradePopupManager — popup won't appear!");

        Debug.Log($"AdTimerManager: Scene changed to {scene.name}");
        adShownThisCycle = false;

        // Check immediately when returning to UI scene
        if (IsUIScene())
            TryShowAd();
    }


    private void Update()
    {
        globalTime += Time.deltaTime;

        // UI Scene timer
        if (IsUIScene())
        {
            uiSceneTime += Time.deltaTime;

            // Check during idle time in menu
            if (globalTime >= AdTriggerTime && !adShownThisCycle)
                TryShowAd();
        }
        // Game Scene timer
        else if (IsGameScene())
        {
            gameSceneTime += Time.deltaTime;
        }
    }

    private bool IsUIScene()
    {
        return currentScene.Equals("ui scene", System.StringComparison.OrdinalIgnoreCase);
    }

    private bool IsGameScene()
    {
        return !IsUIScene(); // Any non-UI scene is considered a game scene here
    }

    private void TryShowAd()
    {
        if (!IsUIScene()) return; // Show ads only in UI scene
        if (adShownThisCycle) return;

        if (globalTime >= AdTriggerTime)
        {
            if (interstitialAd != null && interstitialAd._adLoaded)
            {
                Debug.Log($"AdTimerManager: Showing ad. Total={globalTime:F1}s | UI={uiSceneTime:F1}s | Game={gameSceneTime:F1}s");
                interstitialAd.ShowAd();
                adsShownCount++;
                if (adsShownCount % 4 == 0 && popupManager != null)
                {
                    Debug.Log("Showing upgrade popup after 4th ad.");
                    popupManager.ShowPopup();
                }


                globalTime = 0f;
                uiSceneTime = 0f;
                gameSceneTime = 0f;
                adShownThisCycle = true;
            }
            else
            {
                Debug.Log("AdTimerManager: Ad not ready. Loading for next cycle.");
                interstitialAd?.LoadAd();
            }
        }
    }
}
