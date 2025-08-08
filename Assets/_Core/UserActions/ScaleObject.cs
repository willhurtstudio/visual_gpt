using UnityEngine;

public class ScaleObject : MonoBehaviour, IUserAction
{
    public Transform target; // NEW: assign in inspector or via code
    public Vector3 scaleTarget = Vector3.one * 2f;

    public UserActionType ActionType => UserActionType.ScaleObject;

    public void Start()
    {
        // Optional: Set initial scale if target is not assigned
        if (target == null)
            target = transform;
    }

    public void Execute()
    {
        if (target != null)
            target.localScale = scaleTarget;
        else
            transform.localScale = scaleTarget;
    }
}
