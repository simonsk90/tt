using TT.Application.Abstractions;
using TT.Domain.Robots;

namespace TT.Application.Commands.CompleteRoute;

/// <summary>
/// Handles the <see cref="CompleteRouteCommand"/>.
/// Updates the robot's GPS position (simulated) before calling <see cref="Robot.CompleteRoute()"/>,
/// which enforces invariants and raises <see cref="TT.Domain.Events.RouteCompletedEvent"/>.
/// </summary>
public sealed class CompleteRouteCommandHandler(
    IRobotRepository repository,
    IEventLogger eventLogger)
    : ICommandHandler<CompleteRouteCommand>
{
    public async Task Handle(CompleteRouteCommand command, CancellationToken cancellationToken)
    {
        eventLogger.Log("Command", $"CompleteRouteCommand received for robot {command.RobotId}");

        var robot = await repository.GetByIdAsync(command.RobotId, cancellationToken)
            ?? throw new InvalidOperationException($"Robot {command.RobotId} not found.");

        // Simulate a final GPS fix arriving before route completion
        robot.UpdateGpsPosition(new GpsCoordinates(55.6761 + Random.Shared.NextDouble() * 0.01,
                                                    12.5683 + Random.Shared.NextDouble() * 0.01));

        // Domain invariants enforced here — GPS is now available, status must be Working
        robot.CompleteRoute();

        eventLogger.Log("Domain Event", "RouteCompleted raised");

        // SaveChangesAsync publishes RouteCompletedEvent → triggers RouteCompletedEventHandler
        await repository.SaveChangesAsync(cancellationToken);
    }
}
