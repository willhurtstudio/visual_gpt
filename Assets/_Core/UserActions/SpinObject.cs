using UnityEngine;

public class SpinObject : MonoBehaviour, IUserAction
{
    public Transform target; // NEW: assign in inspector or via code
    public Vector3 spinAxis = Vector3.up;
    public float spinSpeed = 90f; // degrees per second

    public UserActionType ActionType => UserActionType.SpinObject;


    public void Start()
    {
        // Optional: Set initial scale if target is not assigned
        if (target == null)
            target = transform;
    }

    public void Execute()
    {
        transform.Rotate(spinAxis.normalized * spinSpeed * Time.deltaTime);
    }
}
