using MediatR;

namespace TT.Domain.Abstractions;

/// <summary>
/// Base record for all domain events.
/// Implements <see cref="INotification"/> so MediatR can dispatch them to handlers.
/// Domain events are value objects — immutable records that describe something that happened.
/// </summary>
public abstract record DomainEvent(DateTimeOffset OccurredOn) : INotification
{
    protected DomainEvent() : this(DateTimeOffset.UtcNow) { }
}
