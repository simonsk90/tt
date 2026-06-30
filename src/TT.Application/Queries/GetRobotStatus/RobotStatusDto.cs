using TT.Domain.Robots;

namespace TT.Application.Queries.GetRobotStatus;

/// <summary>
/// Lightweight projection of the Robot aggregate for read operations.
/// This is a DTO — it deliberately does not expose domain behaviour.
/// </summary>
public sealed record RobotStatusDto(
    Guid Id,
    string Name,
    string Status,
    double BatteryLevel,
    string? LastKnownPosition,
    DateTimeOffset? FieldMarkingStartedAt,
    DateTimeOffset? RouteCompletedAt)
{
    public static RobotStatusDto From(Robot robot) => new(
        robot.Id,
        robot.Name,
        robot.Status.ToString(),
        robot.BatteryLevel,
        robot.LastKnownPosition?.ToString(),
        robot.FieldMarkingStartedAt,
        robot.RouteCompletedAt);
}
