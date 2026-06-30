using Hangfire;
using MediatR;
using TT.Application.Abstractions;
using TT.Domain.Events;

namespace TT.Application.EventHandlers;

/// <summary>
/// Handles <see cref="RouteCompletedEvent"/> raised by <see cref="TT.Domain.Robots.Robot.CompleteRoute()"/>.
/// Responsibility: enqueue the statistics push background job.
/// </summary>
public sealed class RouteCompletedEventHandler(
    IBackgroundJobClient jobClient,
    IEventLogger eventLogger)
    : INotificationHandler<RouteCompletedEvent>
{
    public Task Handle(RouteCompletedEvent notification, CancellationToken cancellationToken)
    {
        eventLogger.Log("Event Handler",
            $"RouteCompleted handled for '{notification.RobotName}' — duration: {notification.Duration.TotalSeconds:F1}s");

        var jobId = jobClient.Enqueue<IPushStatisticsJob>(
            j => j.ExecuteAsync(notification.RobotId, CancellationToken.None));

        eventLogger.Log("Background Job", $"PushStatisticsToCloud enqueued (job: {jobId})");

        return Task.CompletedTask;
    }
}

/// <summary>
/// Interface for the statistics push job — keeps Application decoupled from Infrastructure types.
/// </summary>
public interface IPushStatisticsJob
{
    Task ExecuteAsync(Guid robotId, CancellationToken cancellationToken);
}
