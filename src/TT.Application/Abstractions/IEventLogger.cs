namespace TT.Application.Abstractions;

/// <summary>
/// Publishes structured log entries to the real-time event stream (SSE).
/// Implemented by <c>EventLogService</c> in TT.Infrastructure.
/// Injected into Application handlers and Hangfire jobs to drive the frontend terminal.
/// </summary>
public interface IEventLogger
{
    void Log(string category, string message);
}
