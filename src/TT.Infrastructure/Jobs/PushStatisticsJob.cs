using Hangfire;
using TT.Application.Abstractions;
using TT.Application.EventHandlers;

namespace TT.Infrastructure.Jobs;

/// <summary>
/// Hangfire background job: pushes field marking statistics to a cloud analytics endpoint.
/// Enqueued by <see cref="RouteCompletedEventHandler"/> after <c>RouteCompletedEvent</c> is raised.
/// On completion, chains to <see cref="SendNotificationJob"/>.
///
/// Production: call your telemetry API (Azure Monitor, DataDog, etc.) here.
/// </summary>
public sealed class PushStatisticsJob(
    IEventLogger eventLogger,
    IBackgroundJobClient jobClient) : IPushStatisticsJob
{
    public async Task ExecuteAsync(Guid robotId, CancellationToken cancellationToken)
    {
        eventLogger.Log("Background Job", "PushStatisticsToCloud: Serialising telemetry payload...");
        await Task.Delay(1000, cancellationToken);

        eventLogger.Log("Background Job", "PushStatisticsToCloud: POST /api/v1/telemetry → 200 OK");
        await Task.Delay(500, cancellationToken);

        jobClient.Enqueue<SendNotificationJob>(j => j.ExecuteAsync(robotId, CancellationToken.None));
        eventLogger.Log("Background Job", "SendNotificationJob enqueued");
    }
}
