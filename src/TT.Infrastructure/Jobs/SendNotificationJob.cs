using TT.Application.Abstractions;
using TT.Application.EventHandlers;

namespace TT.Infrastructure.Jobs;

/// <summary>
/// Hangfire background job: sends an operator notification after a route completes.
/// Enqueued independently by <see cref="RouteCompletedSendNotificationHandler"/>.
///
/// SINGLE RESPONSIBILITY: notification dispatch only.
/// This job is not aware of — and not dependent on — <see cref="PushStatisticsJob"/>.
/// It runs in parallel with statistics, not after it.
///
/// Production: integrate with push notification service, email, Teams webhook, etc.
/// </summary>
public sealed class SendNotificationJob(IEventLogger eventLogger) : ISendNotificationJob
{
    public async Task ExecuteAsync(Guid robotId, CancellationToken cancellationToken)
    {
        eventLogger.Log("Background Job", "SendNotificationCommand: Dispatching operator alert...");
        await Task.Delay(700, cancellationToken);

        eventLogger.Log("✅ Complete", $"Notification sent for robot {robotId}. Job complete! 🚀");
    }
}
