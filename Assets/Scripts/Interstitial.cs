using UnityEngine;
using UnityEngine.Advertisements;
using System.Collections;

public class Interstitial : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] string _androidAdUnitId = "Android_Interstitial";
    [SerializeField] string _iOSAdUnitId = "iOS_Interstitial";
    string _adUnitId;
    
    // CHANGE: Make this public so AdTimerManager can check the status.
    public bool _adLoaded = false; 

    void Awake()
    {
        // Get the Ad Unit ID for the current platform:
        _adUnitId = (Application.platform == RuntimePlatform.IPhonePlayer) 
        ? _iOSAdUnitId
        : _androidAdUnitId;
    }

    void Start()
    {
        // Start loading the first ad after a short delay
        StartCoroutine(InitialLoadDelay());
    }

    IEnumerator InitialLoadDelay()
    {
        // Wait for 1 second to give the AdsInitializer time to complete
        yield return new WaitForSeconds(1f);
        LoadAd();
    }
 
    public void LoadAd()
    {
        Debug.Log("Loading Ad: " + _adUnitId);
        Advertisement.Load(_adUnitId, this);
    }
 
    public void ShowAd()
    {
        // --- CRITICAL FIX: Check the public load status flag ---
        if (_adLoaded)
        {
            Debug.Log("Showing Ad: " + _adUnitId);
            Advertisement.Show(_adUnitId, this);
            _adLoaded = false; // Set to false immediately as the ad is now being shown/consumed
        }
        else
        {
            Debug.Log("Interstitial ad is NOT ready. Attempting to reload for next use.");
            // If it's not ready, try loading it again immediately for the *next* time.
            LoadAd(); 
        }
    }
 
    // IUnityAdsLoadListener implementation
    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        Debug.Log("Ad Loaded: " + adUnitId);
        // FIX: Set the flag to true when the load is successful
        _adLoaded = true;
    }
 
    public void OnUnityAdsFailedToLoad(string _adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Error loading Ad Unit: {_adUnitId} - {error.ToString()} - {message}");
        _adLoaded = false;
        // Optionally try loading again after a delay if error is not NO_FILL
    }
 
    // IUnityAdsShowListener implementation
    public void OnUnityAdsShowFailure(string _adUnitId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Error showing Ad Unit {_adUnitId}: {error.ToString()} - {message}");
        _adLoaded = false;
        LoadAd();
    }
 
    public void OnUnityAdsShowStart(string _adUnitId) { }
    public void OnUnityAdsShowClick(string _adUnitId) { }
    
    public void OnUnityAdsShowComplete(string _adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log("Ad shown complete. Reloading next ad.");
        LoadAd();
    }
}
