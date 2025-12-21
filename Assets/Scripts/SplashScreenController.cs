using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SplashScreenController : MonoBehaviour
{
    [Header("Assign your UI elements")]
    public Image logoImage;
    public Image blackPanel;

    [Header("Timing")]
    public float totalDuration = 3f;          // Total splash length (fade in + fade out)
    public float fadeInRatio = 0.2f;          // % of time spent fading in
    public float fadeOutRatio = 0.5f;         // % of time spent fading out
    public string nextSceneName = "ui Scene";  // Your main UI scene

    [Header("Zoom Effect")]
    public float startScale = 2.0f;           // Start size
    public float endScale = 2.05f;            // Barely larger (subtle zoom)

    private void Start()
    {
        float referenceHeight = 2340f;
        float scaleFactor = Screen.height / referenceHeight;
        logoImage.rectTransform.localScale *= scaleFactor;
        StartCoroutine(PlaySplashSequence());
    }

    IEnumerator PlaySplashSequence()
    {
        // Ensure initial setup
        SetAlpha(logoImage, 0f);
        SetAlpha(blackPanel, 1f);
        logoImage.rectTransform.localScale = Vector3.one * startScale;

        float fadeInTime = totalDuration * fadeInRatio;
        float fadeOutTime = totalDuration * fadeOutRatio;
        float holdTime = totalDuration - (fadeInTime + fadeOutTime);

        float t = 0f;

        // Fade in + slow zoom starts immediately
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, t / fadeInTime);
            SetAlpha(logoImage, progress);
            logoImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, t / totalDuration);
            yield return null;
        }

        // Slight hold (logo visible, zoom continues very slowly)
        t = 0f;
        while (t < holdTime)
        {
            t += Time.deltaTime;
            float zoomProgress = (fadeInTime + t) / totalDuration;
            logoImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, zoomProgress);
            yield return null;
        }

        // Fade out (zoom still continues slightly)
        t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, t / fadeOutTime);
            SetAlpha(logoImage, 1f - progress);
            float zoomProgress = (fadeInTime + holdTime + t) / totalDuration;
            logoImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, zoomProgress);
            yield return null;
        }

        SceneManager.LoadScene(nextSceneName);
    }

    void SetAlpha(Image img, float a)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}
