using UnityEngine;
using System.Linq;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(AudioSource))]
public class MicrophoneManager : MonoBehaviour
{
    public int sampleRate = 48000;
    public int bufferSeconds = 1;
    public int fftSize = 512;
    public FFTWindow window = FFTWindow.BlackmanHarris;

    public bool autoStart = true;
    public int selectedIndex = 0;
    public float updateRate = 1f / 30f;
    public float smoothing = 0.9f;

    [Header("Event Outputs")]
    public FloatEventChannelSO inputLevelEvent;
    public FloatEventChannelSO bassLevelEvent;
    public FloatEventChannelSO midLevelEvent;
    public FloatEventChannelSO highLevelEvent;

    AudioSource src;
    AudioClip clip;
    float nextUpdate;

    float minLevel = 0.0001f;
    float maxLevel = 0.05f;

    float[] spectrum;

    void Awake()
    {
        src = GetComponent<AudioSource>();
        spectrum = new float[fftSize];
        if (autoStart) StartMicrophone();
    }

    void Update()
    {
        if (!src || !src.isPlaying) return;

        if (Time.time >= nextUpdate)
        {
            nextUpdate = Time.time + updateRate;
            UpdateSpectrum();
        }
    }

    void UpdateSpectrum()
    {
        src.GetSpectrumData(spectrum, 0, window);

        float bass = SumRange(0, 15);
        float mids = SumRange(16, 60);
        float highs = SumRange(61, fftSize - 1);

        float total = bass + mids + highs;

        AutoCalibrate(total);

        float norm = Normalize(total);
        float nBass = Normalize(bass);
        float nMids = Normalize(mids);
        float nHighs = Normalize(highs);

        inputLevelEvent?.Raise(norm);
        bassLevelEvent?.Raise(nBass);
        midLevelEvent?.Raise(nMids);
        highLevelEvent?.Raise(nHighs);
    }

    float SumRange(int start, int end)
    {
        float sum = 0f;
        for (int i = start; i <= end && i < spectrum.Length; i++)
            sum += spectrum[i];
        return sum;
    }

    void AutoCalibrate(float level)
    {
        minLevel = Mathf.Min(minLevel * 0.99f + level * 0.01f, minLevel);
        maxLevel = Mathf.Max(maxLevel * 0.99f, level);
    }

    float Normalize(float value)
    {
        float norm = Mathf.InverseLerp(minLevel, maxLevel, value);
        return Mathf.Pow(norm, 0.5f); // perceptual shaping
    }

    public void StartMicrophone()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("No microphones found");
            return;
        }

        selectedIndex = Mathf.Clamp(selectedIndex, 0, Microphone.devices.Length - 1);
        string device = Microphone.devices[selectedIndex];

        clip = Microphone.Start(device, true, bufferSeconds, sampleRate);
        src.clip = clip;
        src.loop = true;
        while (Microphone.GetPosition(device) <= 0) { }
        src.Play();
    }

    public void StopMicrophone()
    {
        if (clip != null)
        {
            src.Stop();
            Microphone.End(null);
        }
    }
}
