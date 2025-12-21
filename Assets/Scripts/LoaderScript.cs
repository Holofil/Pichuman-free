using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoaderScript : MonoBehaviour
{
    void Start()
    {
        // Start loading game scene
        StartCoroutine(LoadGameScene());
    }

    IEnumerator LoadGameScene()
    {
        // OPTIONAL: Loading delay
        yield return new WaitForSeconds(1f);

        // Load the Game Scene (as SINGLE, so old scene unload hoga)
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("game scene", LoadSceneMode.Single);

        while (!asyncLoad.isDone)
        {
            // Optional: Show loading progress
            yield return null;
        }
    }
}
