using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// Lets users pick multiple actions (enum) to execute when a Void event fires.
/// This is the decoupled "router" between triggers (e.g., VolumeThreshold) and effects.
/// </summary>
// public enum UserActionType {
//     ChangeBackgroundColor,
//     ScaleObject,
//     SpinObject
// }

// public interface IUserAction { UserActionType ActionType { get; } void Execute(); }

// public class ChangeBackgroundColor : MonoBehaviour, IUserAction {
//     public Camera targetCamera;
//     public Color color = Color.red;
//     public UserActionType ActionType => UserActionType.ChangeBackgroundColor;
//     public void Execute(){ if (targetCamera) targetCamera.backgroundColor = color; }
// }
// public class ScaleObject : MonoBehaviour, IUserAction {
//     public Transform target;
//     public Vector3 scale = new Vector3(2,2,2);
//     public UserActionType ActionType => UserActionType.ScaleObject;
//     public void Execute(){ if (target) target.localScale = scale; }
// }
// public class SpinObject : MonoBehaviour, IUserAction {
//     public Transform target;
//     public float speed = 180f;
//     public UserActionType ActionType => UserActionType.SpinObject;
//     public void Execute(){ if (target) target.Rotate(Vector3.up, speed * Time.deltaTime); }
// }

public class RuntimeActionRouter : MonoBehaviour {
    [Header("Trigger")]
    public VoidEventChannelSO triggerEvent;

    [Header("Selection (editable at runtime)")]
    public List<UserActionType> selected = new List<UserActionType>();

    [Header("Action Implementations on this GameObject")]
    public List<MonoBehaviour> actionScripts = new List<MonoBehaviour>();

    Dictionary<UserActionType, IUserAction> map = new Dictionary<UserActionType, IUserAction>();

    void Awake(){
        map.Clear();
        foreach (var mb in actionScripts){
            if (mb is IUserAction a) map[a.ActionType] = a;
        }
    }
    void OnEnable(){ if (triggerEvent!=null) triggerEvent.OnRaised += OnTrigger; }
    void OnDisable(){ if (triggerEvent!=null) triggerEvent.OnRaised -= OnTrigger; }

    void OnTrigger(){
        foreach (var t in selected){
            if (map.TryGetValue(t, out var a)) a.Execute();
        }
    }
}
