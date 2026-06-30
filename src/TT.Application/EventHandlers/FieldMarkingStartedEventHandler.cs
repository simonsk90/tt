using Hangfire;
using MediatR;
using TT.Application.Abstractions;
using TT.Domain.Events;

namespace TT.Application.EventHandlers;

/// <summary>
/// Handles <see cref="FieldMarkingStartedEvent"/> raised by <see cref="TT.Domain.Robots.Robot.StartFieldMarking()"/>.
/// Responsibility: enqueue the GPS tracking background job via Hangfire.
/// This handler lives in Application — it knows about MediatR and the job interface but NOT EF Core.
/// </summary>
public sealed class FieldMarkingStartedEventHandler(
    IBackgroundJobClient jobClient,
    IEventLogger eventLogger)
    : INotificationHandler<FieldMarkingStartedEvent>
{
    public Task Handle(FieldMarkingStartedEvent notification, CancellationToken cancellationToken)
    {
        eventLogger.Log("Event Handler", $"FieldMarkingStarted handled for robot '{notification.RobotName}'");

        var jobId = jobClient.Enqueue<ITrackGpsProgressJob>(
            j => j.ExecuteAsync(notification.RobotId, CancellationToken.None));

        eventLogger.Log("Background Job", $"TrackGpsProgress enqueued (job: {jobId})");

        return Task.CompletedTask;
    }
}

/// <summary>
/// Interface for the GPS tracking job — defined in Application to avoid coupling to Infrastructure types.
/// </summary>
public interface ITrackGpsProgressJob
{
    Task ExecuteAsync(Guid robotId, CancellationToken cancellationToken);
}
