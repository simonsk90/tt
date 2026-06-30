using TT.Domain.Abstractions;
using TT.Domain.Robots;

namespace TT.Domain.Events;

/// <summary>
/// Raised when a robot successfully begins a field marking operation.
/// Triggers: Hangfire job <c>TrackGpsProgressJob</c> (via Application event handler).
/// </summary>
public sealed record FieldMarkingStartedEvent(
    Guid RobotId,
    string RobotName,
    DateTimeOffset StartedAt) : DomainEvent(StartedAt);
