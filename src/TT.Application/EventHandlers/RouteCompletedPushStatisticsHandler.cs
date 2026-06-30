using Hangfire;
using MediatR;
using TT.Application.Abstractions;
using TT.Domain.Events;

namespace TT.Application.EventHandlers;

/// <summary>
/// Handles <see cref="RouteCompletedEvent"/> — responsibility: telemetry only.
/// Enqueues <see cref="IPushStatisticsJob"/> to push field marking data to the cloud analytics endpoint.
///
/// MediatR broadcasts <see cref="RouteCompletedEvent"/> to ALL registered handlers concurrently.
/// This handler is completely independent of <see cref="RouteCompletedSendNotificationHandler"/>.
/// If this handler or its job fails, the notification pipeline is unaffected.
/// </summary>
public sealed class RouteCompletedPushStatisticsHandler(
    IBackgroundJobClient jobClient,
    IEventLogger eventLogger)
    : INotificationHandler<RouteCompletedEvent>
{
    public Task Handle(RouteCompletedEvent notification, CancellationToken cancellationToken)
    {
        eventLogger.Log("Event Handler",
            $"RouteCompleted → PushStatistics handler for '{notification.RobotName}' (duration: {notification.Duration.TotalSeconds:F1}s)");

        var jobId = jobClient.Enqueue<IPushStatisticsJob>(
            j => j.ExecuteAsync(notification.RobotId, CancellationToken.None));

        eventLogger.Log("Background Job", $"PushStatisticsToCloud enqueued (job: {jobId})");

        return Task.CompletedTask;
    }
}

/// <summary>
/// Interface for the statistics push job.
/// Defined in Application so Infrastructure remains decoupled from handler code.
/// </summary>
public interface IPushStatisticsJob
{
    Task ExecuteAsync(Guid robotId, CancellationToken cancellationToken);
}
