using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SplashBootLoader : MonoBehaviour
{
    IEnumerator Start()
    {
        // Give Unity a few frames to stabilize after splash
        yield return new WaitForSecondsRealtime(0.3f);

        // Start loading your actual UI scene asynchronously (no hiccup)
        AsyncOperation load = SceneManager.LoadSceneAsync("ui Scene");
        load.allowSceneActivation = false;

        // Let splash fully fade before activating
        yield return new WaitForSecondsRealtime(1f);
        load.allowSceneActivation = true;
    }
}
