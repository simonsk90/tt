using TT.Domain.Abstractions;
using TT.Domain.Events;
using TT.Domain.Robots;

namespace TT.Domain.Robots;

/// <summary>
/// Robot is the Aggregate Root for the field-marking domain.
/// All state mutations MUST go through the public methods on this class.
/// External code must never set properties directly — this enforces domain invariants.
///
/// AI AGENTS: Before modifying this class, read .github/topics/RouteCompleteTopic.md.
/// </summary>
public sealed class Robot : AggregateRoot
{
    // Private parameterless constructor for EF Core materialisation.
    private Robot() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public RobotStatus Status { get; private set; }
    public double BatteryLevel { get; private set; }
    public GpsCoordinates? LastKnownPosition { get; private set; }
    public DateTimeOffset? FieldMarkingStartedAt { get; private set; }
    public DateTimeOffset? RouteCompletedAt { get; private set; }

    private const double MinimumBatteryLevelPercent = 20.0;

    /// <summary>
    /// Factory method — the only way to create a valid Robot aggregate.
    /// </summary>
    public static Robot Create(Guid id, string name, double batteryLevel, GpsCoordinates? initialPosition = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (batteryLevel is < 0 or > 100)
            throw new DomainException("Battery level must be between 0 and 100.");

        return new Robot
        {
            Id = id,
            Name = name,
            BatteryLevel = batteryLevel,
            LastKnownPosition = initialPosition,
            Status = RobotStatus.Idle
        };
    }

    /// <summary>
    /// Starts the field marking operation.
    /// Invariants:
    ///   - Robot must NOT already be working.
    ///   - Battery must be at least <see cref="MinimumBatteryLevelPercent"/>%.
    /// Raises: <see cref="FieldMarkingStartedEvent"/>
    /// </summary>
    public void StartFieldMarking()
    {
        if (Status == RobotStatus.Working)
            throw new DomainException($"Robot '{Name}' is already performing a field marking operation.");

        if (BatteryLevel < MinimumBatteryLevelPercent)
            throw new DomainException(
                $"Insufficient battery ({BatteryLevel:F1}%). At least {MinimumBatteryLevelPercent}% is required to start field marking.");

        Status = RobotStatus.Working;
        FieldMarkingStartedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new FieldMarkingStartedEvent(Id, Name, FieldMarkingStartedAt.Value));
    }

    /// <summary>
    /// Completes the current route and transitions the robot back to Idle.
    /// Invariants:
    ///   - Robot MUST be in Working state (route was started).
    ///   - GPS data MUST be available (robot must have reported its position).
    /// Raises: <see cref="RouteCompletedEvent"/>
    /// </summary>
    public void CompleteRoute()
    {
        if (Status != RobotStatus.Working)
            throw new DomainException($"Cannot complete route for robot '{Name}' — route was never started.");

        if (LastKnownPosition is null)
            throw new DomainException($"Cannot complete route for robot '{Name}' — GPS data is unavailable.");

        var completedAt = DateTimeOffset.UtcNow;
        Status = RobotStatus.Idle;
        RouteCompletedAt = completedAt;

        RaiseDomainEvent(new RouteCompletedEvent(
            Id, Name,
            FieldMarkingStartedAt!.Value,
            completedAt,
            LastKnownPosition));
    }

    /// <summary>
    /// Called by the GPS tracking job to update the robot's known position.
    /// Does not raise a domain event — positional updates are operational, not business events.
    /// </summary>
    public void UpdateGpsPosition(GpsCoordinates position) =>
        LastKnownPosition = position;

    /// <summary>
    /// Called by the infrastructure when a battery update is received from the robot.
    /// </summary>
    public void UpdateBatteryLevel(double batteryLevel)
    {
        if (batteryLevel is < 0 or > 100)
            throw new DomainException("Battery level must be between 0 and 100.");
        BatteryLevel = batteryLevel;
    }
}
