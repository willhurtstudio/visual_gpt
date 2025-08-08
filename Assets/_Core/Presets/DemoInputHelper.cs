using UnityEngine;

public class DemoInputHelper : MonoBehaviour
{
    [Header("Core References")]
    public MicrophoneManager micManager;
    public VolumeThreshold bassThreshold;

    [Header("Event Channels")]
    public FloatEventChannelSO bassLevel;
    public VoidEventChannelSO thresholdPassed;

    [Header("Visual Target")]
    public ChangeBackgroundColor bgAction;

    void Start()
    {
        if (micManager != null)
        {
            micManager.bassLevelEvent = bassLevel;
            micManager.StartMicrophone();
        }

        if (bassThreshold != null)
        {
            bassThreshold.inputSignal = bassLevel;
            bassThreshold.onThresholdPassed = thresholdPassed;
        }

        if (bgAction != null)
        {
            thresholdPassed.OnRaised += bgAction.Execute;
        }
    }

    void OnDestroy()
    {
        if (thresholdPassed != null && bgAction != null)
        {
            thresholdPassed.OnRaised -= bgAction.Execute;
        }
    }
}
