using UnityEngine;
/// <summary>
/// A minimal visual preset that can be authored in-editor and applied at runtime.
/// Extend this with more parameters as needed.
/// </summary>
[CreateAssetMenu(menuName = "Presets/Visual Preset")]
public class VisualPresetSO : ScriptableObject {
    public bool affectsBackground = true;
    public Color backgroundColor = Color.black;

    public bool affectsTransform = false;
    public Vector3 targetScale = Vector3.one;
    public float spinSpeed = 0f;
}
