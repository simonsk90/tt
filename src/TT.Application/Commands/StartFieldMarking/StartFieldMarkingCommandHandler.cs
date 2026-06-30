using TT.Application.Abstractions;
using TT.Domain.Robots;

namespace TT.Application.Commands.StartFieldMarking;

/// <summary>
/// Handles the <see cref="StartFieldMarkingCommand"/>.
/// Orchestrates the domain operation without containing any business logic itself.
/// Business rules live in <see cref="Robot.StartFieldMarking()"/>.
/// </summary>
public sealed class StartFieldMarkingCommandHandler(
    IRobotRepository repository,
    IEventLogger eventLogger)
    : ICommandHandler<StartFieldMarkingCommand, Guid>
{
    public async Task<Guid> Handle(StartFieldMarkingCommand command, CancellationToken cancellationToken)
    {
        eventLogger.Log("Command", $"StartFieldMarkingCommand received for '{command.RobotName}'");

        var robotId = command.RobotId ?? Guid.NewGuid();
        var robot = await repository.GetByIdAsync(robotId, cancellationToken);

        if (robot is null)
        {
            robot = Robot.Create(robotId, command.RobotName, command.BatteryLevel);
            await repository.AddAsync(robot, cancellationToken);
            eventLogger.Log("Domain", $"Robot '{robot.Name}' registered (ID: {robot.Id})");
        }

        // Domain invariants enforced inside StartFieldMarking() — may throw DomainException
        robot.StartFieldMarking();

        eventLogger.Log("Domain Event", "FieldMarkingStarted raised");

        // SaveChangesAsync in AppDbContext intercepts DomainEvents and publishes via MediatR
        await repository.SaveChangesAsync(cancellationToken);

        return robotId;
    }
}
