using UnityEngine;
using System.Collections;
/// <summary>
/// Applies VisualPresetSO to bound targets and supports smooth crossfades between presets.
/// Bind references in the inspector (camera/transform).
/// </summary>
public class PresetController : MonoBehaviour {
    [Header("Bindings")]
    public Camera targetCamera;
    public Transform targetTransform;

    [Header("Runtime")]
    public VisualPresetSO current;
    public VisualPresetSO next;

    private Coroutine _fadeRoutine;

    public void Apply(VisualPresetSO preset) {
        current = preset;
        ApplyInstant(preset);
    }

    public void CrossfadeTo(VisualPresetSO preset, float duration){
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeRoutine(preset, duration));
    }

    private void ApplyInstant(VisualPresetSO p){
        if (p == null) return;
        if (p.affectsBackground && targetCamera != null)
            targetCamera.backgroundColor = p.backgroundColor;
        if (p.affectsTransform && targetTransform != null) {
            targetTransform.localScale = p.targetScale;
            // spinSpeed is applied in Update for continuous rotation
            next = p;
        }
    }

    IEnumerator FadeRoutine(VisualPresetSO p, float duration){
        if (p == null) yield break;
        VisualPresetSO from = current;
        float t = 0f;
        Color fromCol = (from!=null)? from.backgroundColor : (targetCamera? targetCamera.backgroundColor : Color.black);
        Vector3 fromScale = (from!=null)? from.targetScale : (targetTransform? targetTransform.localScale : Vector3.one);

        while (t < 1f){
            t += Time.deltaTime / Mathf.Max(0.0001f, duration);
            float k = Mathf.SmoothStep(0f, 1f, t);
            if (targetCamera && p.affectsBackground) {
                targetCamera.backgroundColor = Color.Lerp(fromCol, p.backgroundColor, k);
            }
            if (targetTransform && p.affectsTransform) {
                targetTransform.localScale = Vector3.Lerp(fromScale, p.targetScale, k);
            }
            yield return null;
        }
        current = p;
        next = p;
    }

    void Update(){
        // Apply continuous spin if preset requests it
        var p = current;
        if (p != null && p.affectsTransform && Mathf.Abs(p.spinSpeed) > 0.01f && targetTransform != null){
            targetTransform.Rotate(Vector3.up, p.spinSpeed * Time.deltaTime, Space.Self);
        }
    }
}
