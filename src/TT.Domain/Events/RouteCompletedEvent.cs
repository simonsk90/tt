using TT.Domain.Abstractions;
using TT.Domain.Robots;

namespace TT.Domain.Events;

/// <summary>
/// Raised when a robot successfully completes its assigned route.
/// Invariants enforced before this event is raised:
///   - Robot must have been in <see cref="RobotStatus.Working"/> state (i.e. route was started).
///   - GPS data must be available at the time of completion.
///   - Battery must not have been depleted mid-route (validated before CompleteRoute is called).
/// Triggers: Hangfire jobs <c>PushStatisticsJob</c> → <c>SendNotificationJob</c>.
/// </summary>
public sealed record RouteCompletedEvent(
    Guid RobotId,
    string RobotName,
    DateTimeOffset StartedAt,
    DateTimeOffset CompletedAt,
    GpsCoordinates FinalPosition) : DomainEvent(CompletedAt)
{
    public TimeSpan Duration => CompletedAt - StartedAt;
}
