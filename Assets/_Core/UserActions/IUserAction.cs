public enum UserActionType
{
    ChangeBackgroundColor,
    ScaleObject,
    SpinObject
}

public interface IUserAction
{
    UserActionType ActionType { get; }
    void Execute();
}
