using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

/// <summary>
/// Handles ad timing using three coordinated timers:
/// 1. Global timer → Tracks total time since game start
/// 2. Menu (UI) timer → Tracks time spent in UI scene
/// 3. Game timer → Tracks time spent in gameplay scene
///
/// Ads are shown only in UI scene, once every 90 seconds.
/// </summary>
public class AdTimerManager : MonoBehaviour
{
    private const float AdTriggerTime = 90f; // 90 seconds between ads

    private float globalTime = 0f;
    private float uiSceneTime = 0f;
    private float gameSceneTime = 0f;

    private int adsShownCount = 0;
    private bool adShownThisCycle = false;

    private string currentScene;
    private Interstitial interstitialAd;
    private UpgradePopupManager popupManager;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        interstitialAd = FindObjectOfType<Interstitial>();
        if (interstitialAd == null)
            Debug.LogWarning("AdTimerManager: Interstitial not found.");

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

        popupManager = Resources.FindObjectsOfTypeAll<UpgradePopupManager>()
            .FirstOrDefault(m => m.gameObject.scene.isLoaded);

        adShownThisCycle = false;

        if (IsUIScene())
            TryShowAd();
    }

    private void Update()
    {
        globalTime += Time.deltaTime;
        Debug.Log($"[AdTimer] Global={globalTime:F1}, Shown={adShownThisCycle}");

        if (IsUIScene())
        {
            uiSceneTime += Time.deltaTime;

            if (globalTime >= AdTriggerTime && !adShownThisCycle)
                TryShowAd();
        }
        else
        {
            gameSceneTime += Time.deltaTime;
        }
    }

    private bool IsUIScene()
    {
        return currentScene.Equals("ui scene", System.StringComparison.OrdinalIgnoreCase);
    }

    private void TryShowAd()
    {
        if (!IsUIScene()) return;
        if (adShownThisCycle) return;

        if (globalTime < AdTriggerTime) return;

        if (interstitialAd != null && interstitialAd._adLoaded)
        {
            Debug.Log(
                $"AdTimerManager: Showing ad | Global={globalTime:F1}s UI={uiSceneTime:F1}s Game={gameSceneTime:F1}s"
            );

            interstitialAd.ShowAd();
            adsShownCount++;

            if (adsShownCount >= 4 && popupManager != null)
            {
                popupManager.ShowPopup();
                adsShownCount = 0;
            }

            // Reset timers for next cycle
            globalTime = 0f;
            uiSceneTime = 0f;
            gameSceneTime = 0f;

            // 🔑 IMPORTANT FIX: re-arm the next ad cycle
            adShownThisCycle = false;
        }
        else
        {
            Debug.Log("AdTimerManager: Ad not ready. Requesting load.");
            interstitialAd?.LoadAd();
        }
    }
}
