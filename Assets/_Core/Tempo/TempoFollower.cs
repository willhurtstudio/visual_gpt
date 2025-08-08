using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// TempoFollower estimates tempo (BPM) from incoming audio using spectral flux on FFT.
/// - Designed for line-level electronic music: steady kicks & percussive content.
/// - Emits current BPM and confidence via events for UI/logic to consume.
/// - Can be blended with TapTempo as a fallback (external override).
///
/// Attach to a GameObject with an AudioSource (mic routed) and assign bpmEvent/confidenceEvent.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class TempoFollower : MonoBehaviour {
    public FloatEventChannelSO bpmEvent;
    public FloatEventChannelSO confidenceEvent;
    public bool useMicAudioSource = true;

    [Header("FFT Settings")]
    public int fftSize = 1024;
    public FFTWindow window = FFTWindow.BlackmanHarris;

    [Header("Peak Picking")]
    [Tooltip("Multiplier for adaptive threshold (higher = fewer peaks).")]
    public float thresholdMul = 1.5f;
    [Tooltip("Window for adaptive threshold (frames).")]
    public int thresholdWindow = 20;

    [Header("Smoothing")]
    [Tooltip("Smoothing of BPM output. 0=no smoothing, 1=very slow.")]
    [Range(0f,1f)] public float bpmSmoothing = 0.85f;

    [Header("Limits")]
    public float minBPM = 70f;
    public float maxBPM = 180f;

    private AudioSource _src;
    private float[] _spectrumA;
    private float[] _spectrumB;
    private float[] _prev;
    private List<float> _fluxHistory = new List<float>(512);
    private List<float> _peakTimes = new List<float>(64);
    private float _bpm = 0f;
    private float _conf = 0f;

    void Awake() {
        _src = GetComponent<AudioSource>();
        _spectrumA = new float[fftSize];
        _spectrumB = new float[fftSize];
        _prev = new float[fftSize];
    }

    void Update() {
        if (_src == null || !_src.isPlaying) return;

        // Double-buffer spectrum to reduce allocation jitter
        _src.GetSpectrumData(_spectrumA, 0, window);
        float flux = 0f;
        for (int i = 0; i < _spectrumA.Length; i++) {
            float v = Mathf.Max(0f, _spectrumA[i] - _prev[i]);
            flux += v;
            _prev[i] = _spectrumA[i];
        }
        _fluxHistory.Add(flux);
        if (_fluxHistory.Count > 1024) _fluxHistory.RemoveAt(0);

        // Adaptive thresholding
        int w = Mathf.Max(4, thresholdWindow);
        float thr = 0f;
        int start = Mathf.Max(0, _fluxHistory.Count - w);
        for (int i = start; i < _fluxHistory.Count; i++) thr += _fluxHistory[i];
        thr = (thr / Mathf.Max(1, _fluxHistory.Count - start)) * thresholdMul;

        bool isPeak = flux > thr;
        if (isPeak) {
            _peakTimes.Add(Time.time);
            if (_peakTimes.Count > 32) _peakTimes.RemoveAt(0);
            EstimateBPM();
        }

        // Emit smoothed BPM/confidence
        if (bpmEvent != null) bpmEvent.Raise(_bpm);
        if (confidenceEvent != null) confidenceEvent.Raise(_conf);
    }

    void EstimateBPM() {
        if (_peakTimes.Count < 4) return;
        // Compute IOIs
        List<float> iois = new List<float>(_peakTimes.Count - 1);
        for (int i = 1; i < _peakTimes.Count; i++) {
            iois.Add(_peakTimes[i] - _peakTimes[i-1]);
        }
        // Robust median
        iois.Sort();
        float median = iois[iois.Count / 2];
        if (median <= 1e-3f) return;

        float bpmRaw = 60f / median;
        // Normalize to [minBPM, maxBPM] by doubling/halving
        while (bpmRaw < minBPM) bpmRaw *= 2f;
        while (bpmRaw > maxBPM) bpmRaw *= 0.5f;

        // Confidence: inverse dispersion of IOIs
        float mean = 0f;
        for (int i = 0; i < iois.Count; i++) mean += iois[i];
        mean /= iois.Count;
        float var = 0f;
        for (int i = 0; i < iois.Count; i++) {
            float d = iois[i] - mean;
            var += d*d;
        }
        var /= Mathf.Max(1, iois.Count-1);
        float std = Mathf.Sqrt(var);
        float c = Mathf.Clamp01(1f - (std / Mathf.Max(1e-3f, mean)));

        // Exponential smoothing
        _bpm = (_bpmSmoothing() * _bpm) + ((1f - _bpmSmoothing()) * bpmRaw);
        _conf = (_bpmSmoothing() * _conf) + ((1f - _bpmSmoothing()) * c);
    }

    float _bpmSmoothing() => Mathf.Clamp01(bpmSmoothing);
}
