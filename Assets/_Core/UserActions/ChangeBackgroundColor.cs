using UnityEngine;

public class ChangeBackgroundColor : MonoBehaviour, IUserAction
{
    public Camera targetCamera;
    public Color color = Color.red;

    public UserActionType ActionType => UserActionType.ChangeBackgroundColor;

    public void Execute()
    {
        if (targetCamera != null)
            targetCamera.backgroundColor = color;
    }
}
