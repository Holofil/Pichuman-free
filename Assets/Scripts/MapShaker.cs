using UnityEngine;

public class MapShaker : MonoBehaviour
{
    public float shakeAmplitude = 0.05f;
    public float shakeFrequency = 5f;
    public bool isShakingActive = false;

    private Vector3 initialPosition;
    private float shakeTimer = 0f;

    private void Start()
    {
        initialPosition = transform.localPosition;
    }

    private void Update()
    {
        if (!isShakingActive)
        {
            if (transform.localPosition != initialPosition)
                transform.localPosition = initialPosition;
            return;
        }

        shakeTimer += Time.deltaTime * shakeFrequency;

        float offsetX = Mathf.PerlinNoise(shakeTimer, 0f) - 0.5f;
        float offsetY = Mathf.PerlinNoise(0f, shakeTimer) - 0.5f;

        Vector3 shakeOffset = new Vector3(offsetX, 0, offsetY) * shakeAmplitude;
        transform.localPosition = initialPosition + shakeOffset;
    }

    public void StopShaking()
    {
        isShakingActive = false;
        transform.localPosition = initialPosition;
    }
}
