using UnityEngine;

public class VolumeThreshold : MonoBehaviour
{
    [Header("Input")]
    public FloatEventChannelSO inputSignal;

    [Header("Trigger Settings")]
    [Range(0f, 1f)] public float threshold = 0.5f;
    public float smoothing = 0.9f;

    [Header("Output")]
    public VoidEventChannelSO onThresholdPassed;

    float currentValue = 0f;
    bool wasAbove = false;

    void OnEnable()
    {
        if (inputSignal != null)
            inputSignal.OnRaised += HandleInput;
    }

    void OnDisable()
    {
        if (inputSignal != null)
            inputSignal.OnRaised -= HandleInput;
    }

    void HandleInput(float value)
    {
        // Exponential smoothing
        currentValue = Mathf.Lerp(currentValue, value, 1f - smoothing);

        bool isAbove = currentValue >= threshold;

        if (!wasAbove && isAbove)
        {
            onThresholdPassed?.Raise();
        }

        wasAbove = isAbove;
    }
}
