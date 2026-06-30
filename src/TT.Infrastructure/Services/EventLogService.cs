using System.Collections.Concurrent;
using System.Threading.Channels;
using TT.Application.Abstractions;

namespace TT.Infrastructure.Services;

/// <summary>
/// Singleton service that acts as the event bus for the frontend terminal log.
/// Implements <see cref="IEventLogger"/> so Application handlers and Hangfire jobs
/// can publish events without depending on this infrastructure type.
///
/// Architecture: uses a <see cref="Channel{T}"/> fan-out pattern.
/// Each SSE connection gets its own <see cref="ChannelReader{T}"/> and receives
/// all events that occurred after subscription. Historical events (before subscription)
/// are replayed immediately on connect.
/// </summary>
public sealed class EventLogService : IEventLogger
{
    private readonly List<EventLogEntry> _history = [];
    private readonly List<Channel<EventLogEntry>> _subscribers = [];
    private readonly Lock _lock = new();

    public void Log(string category, string message)
    {
        var entry = new EventLogEntry(category, message, DateTimeOffset.UtcNow);

        lock (_lock)
        {
            _history.Add(entry);
            foreach (var channel in _subscribers)
                channel.Writer.TryWrite(entry);
        }
    }

    /// <summary>
    /// Subscribe to the event stream. Returns all historical events plus a reader
    /// for subsequent events. Call <see cref="Unsubscribe"/> when the SSE connection closes.
    /// </summary>
    public (IReadOnlyList<EventLogEntry> History, ChannelReader<EventLogEntry> Reader, Channel<EventLogEntry> Channel)
        Subscribe()
    {
        var channel = Channel.CreateUnbounded<EventLogEntry>(
            new UnboundedChannelOptions { SingleReader = true });

        lock (_lock)
        {
            var snapshot = _history.ToList();
            _subscribers.Add(channel);
            return (snapshot, channel.Reader, channel);
        }
    }

    public void Unsubscribe(Channel<EventLogEntry> channel)
    {
        lock (_lock)
        {
            _subscribers.Remove(channel);
            channel.Writer.TryComplete();
        }
    }

    public IReadOnlyList<EventLogEntry> GetHistory()
    {
        lock (_lock) return _history.ToList();
    }
}

public sealed record EventLogEntry(string Category, string Message, DateTimeOffset Timestamp);
