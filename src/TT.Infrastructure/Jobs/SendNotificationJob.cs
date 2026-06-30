using TT.Application.Abstractions;

namespace TT.Infrastructure.Jobs;

/// <summary>
/// Hangfire background job: sends an operator notification once the full pipeline completes.
/// This is the terminal step in the RouteComplete event chain.
/// After this job runs, the frontend terminal displays "Job complete! 🚀".
///
/// Production: integrate with push notification service, email, Teams webhook, etc.
/// </summary>
public sealed class SendNotificationJob(IEventLogger eventLogger)
{
    public async Task ExecuteAsync(Guid robotId, CancellationToken cancellationToken)
    {
        eventLogger.Log("Background Job", "SendNotificationCommand: Dispatching operator alert...");
        await Task.Delay(700, cancellationToken);

        eventLogger.Log("✅ Complete", $"Notification sent for robot {robotId}. Job complete! 🚀");
    }
}
