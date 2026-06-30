using MediatR;
using TT.Application.Abstractions;
using TT.Application.Commands.CompleteRoute;

namespace TT.Infrastructure.Jobs;

/// <summary>
/// Hangfire job that triggers route completion by sending a <see cref="CompleteRouteCommand"/>.
/// Enqueued by <see cref="TrackGpsProgressJob"/> once GPS tracking is finished.
/// Delegates all domain logic to <see cref="CompleteRouteCommandHandler"/> via MediatR.
/// </summary>
public sealed class CompleteRouteJob(IMediator mediator, IEventLogger eventLogger)
{
    public async Task ExecuteAsync(Guid robotId, CancellationToken cancellationToken)
    {
        eventLogger.Log("Background Job", $"CompleteRouteJob: executing for robot {robotId}");
        await mediator.Send(new CompleteRouteCommand(robotId), cancellationToken);
    }
}
