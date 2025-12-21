using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    public float ShakeAmount = 0.1f;
    public float ShakeSpeed = 1f;
    public bool EnableShake = true;

    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    void Update()
    {
        if (!EnableShake) return;

        Vector3 offset = Random.insideUnitSphere * ShakeAmount;
        transform.localPosition = originalPosition + offset;
    }

    private void OnDisable()
    {
        transform.localPosition = originalPosition;
    }
}
