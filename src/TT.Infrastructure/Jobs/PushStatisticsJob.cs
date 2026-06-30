using TT.Application.Abstractions;
using TT.Application.EventHandlers;

namespace TT.Infrastructure.Jobs;

/// <summary>
/// Hangfire background job: pushes field marking statistics to a cloud analytics endpoint.
/// Enqueued independently by <see cref="RouteCompletedPushStatisticsHandler"/>.
///
/// SINGLE RESPONSIBILITY: telemetry serialisation and HTTP dispatch only.
/// This job must NOT enqueue or chain to any other job.
/// Downstream concerns (notifications, billing) are triggered by their own independent
/// MediatR handlers — not by this job.
///
/// Production: call your telemetry API (Azure Monitor, DataDog, etc.) here.
/// </summary>
public sealed class PushStatisticsJob(IEventLogger eventLogger) : IPushStatisticsJob
{
    public async Task ExecuteAsync(Guid robotId, CancellationToken cancellationToken)
    {
        eventLogger.Log("Background Job", "PushStatisticsToCloud: Serialising telemetry payload...");
        await Task.Delay(1000, cancellationToken);

        eventLogger.Log("Background Job", "PushStatisticsToCloud: POST /api/v1/telemetry → 200 OK");
        await Task.Delay(500, cancellationToken);

        eventLogger.Log("Background Job", "PushStatisticsToCloud: Complete ✓");
    }
}
