
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Events/Void Event Channel")]
public class VoidEventChannelSO : ScriptableObject
{
    public event Action OnRaised;
    public void Raise() => OnRaised?.Invoke();
}
