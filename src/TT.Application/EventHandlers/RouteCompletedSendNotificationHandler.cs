using Hangfire;
using MediatR;
using TT.Application.Abstractions;
using TT.Domain.Events;

namespace TT.Application.EventHandlers;

/// <summary>
/// Handles <see cref="RouteCompletedEvent"/> — responsibility: operator notification only.
/// Enqueues <see cref="ISendNotificationJob"/> independently of the statistics pipeline.
///
/// MediatR broadcasts <see cref="RouteCompletedEvent"/> to ALL registered handlers concurrently.
/// This handler is completely independent of <see cref="RouteCompletedPushStatisticsHandler"/>.
/// If the statistics API fails, the operator still receives their notification.
/// </summary>
public sealed class RouteCompletedSendNotificationHandler(
    IBackgroundJobClient jobClient,
    IEventLogger eventLogger)
    : INotificationHandler<RouteCompletedEvent>
{
    public Task Handle(RouteCompletedEvent notification, CancellationToken cancellationToken)
    {
        eventLogger.Log("Event Handler",
            $"RouteCompleted → SendNotification handler for '{notification.RobotName}'");

        var jobId = jobClient.Enqueue<ISendNotificationJob>(
            j => j.ExecuteAsync(notification.RobotId, CancellationToken.None));

        eventLogger.Log("Background Job", $"SendNotificationCommand enqueued (job: {jobId})");

        return Task.CompletedTask;
    }
}

/// <summary>
/// Interface for the operator notification job.
/// Defined in Application so Infrastructure remains decoupled from handler code.
/// </summary>
public interface ISendNotificationJob
{
    Task ExecuteAsync(Guid robotId, CancellationToken cancellationToken);
}
