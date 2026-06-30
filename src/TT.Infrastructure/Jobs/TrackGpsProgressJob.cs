using Hangfire;
using MediatR;
using TT.Application.Abstractions;
using TT.Application.Commands.CompleteRoute;
using TT.Application.EventHandlers;

namespace TT.Infrastructure.Jobs;

/// <summary>
/// Hangfire background job: simulates GPS telemetry collection during a field marking run.
/// Runs after <see cref="FieldMarkingStartedEventHandler"/> enqueues it.
/// When tracking is complete, enqueues <see cref="CompleteRouteJob"/>.
///
/// Production: this would subscribe to a real GPS telemetry stream (MQTT, WebSocket, etc.)
/// </summary>
public sealed class TrackGpsProgressJob(
    IEventLogger eventLogger,
    IBackgroundJobClient jobClient) : ITrackGpsProgressJob
{
    public async Task ExecuteAsync(Guid robotId, CancellationToken cancellationToken)
    {
        eventLogger.Log("Background Job", "TrackGpsProgress: Acquiring GPS lock...");
        await Task.Delay(1200, cancellationToken);

        eventLogger.Log("Background Job", "TrackGpsProgress: GPS lock acquired — tracking route...");
        await Task.Delay(1800, cancellationToken);

        eventLogger.Log("Background Job", "TrackGpsProgress: Field boundary mapped. Route 100% complete.");
        await Task.Delay(600, cancellationToken);

        jobClient.Enqueue<CompleteRouteJob>(j => j.ExecuteAsync(robotId, CancellationToken.None));
        eventLogger.Log("Background Job", "CompleteRouteJob enqueued");
    }
}
