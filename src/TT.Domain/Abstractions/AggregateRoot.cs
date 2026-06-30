namespace TT.Domain.Abstractions;

/// <summary>
/// Base class for all aggregate roots in the domain.
/// Aggregate roots are the only entry point into an aggregate cluster.
/// Domain events are collected here and dispatched by the infrastructure layer
/// inside <c>AppDbContext.SaveChangesAsync</c> — never dispatched directly from domain code.
/// </summary>
public abstract class AggregateRoot
{
    private readonly List<DomainEvent> _domainEvents = [];

    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(DomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
