namespace TT.Domain.Robots;

/// <summary>
/// Represents the operational lifecycle state of a Robot aggregate.
/// State transitions: Idle → Working → Idle.
/// A robot cannot <see cref="Robot.CompleteRoute"/> unless it is in <see cref="Working"/> state.
/// </summary>
public enum RobotStatus
{
    Idle = 0,
    Working = 1,
    Error = 2
}
