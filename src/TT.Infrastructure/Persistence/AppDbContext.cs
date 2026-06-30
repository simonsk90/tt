using MediatR;
using Microsoft.EntityFrameworkCore;
using TT.Domain.Abstractions;
using TT.Domain.Robots;

namespace TT.Infrastructure.Persistence;

/// <summary>
/// In-memory EF Core DbContext for the TT.BackendTransformation demo.
/// Key responsibility: intercept <see cref="SaveChangesAsync"/> to dispatch domain events
/// via MediatR AFTER persistence succeeds. This ensures events are only published
/// for state that has actually been committed.
///
/// Production note: replace <c>UseInMemoryDatabase</c> with a real provider (SQL Server, Postgres).
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options, IPublisher publisher)
    : DbContext(options)
{
    public DbSet<Robot> Robots => Set<Robot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Robot>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).IsRequired().HasMaxLength(100);
            entity.Property(r => r.Status).HasConversion<int>();
            entity.Property(r => r.BatteryLevel);
            entity.Property(r => r.FieldMarkingStartedAt);
            entity.Property(r => r.RouteCompletedAt);

            // Map GpsCoordinates value object as an owned entity
            // HasColumnName is relational-only; omitted for InMemory compatibility
            entity.OwnsOne(r => r.LastKnownPosition);

            // Ignore the domain events collection — never persisted
            entity.Ignore(r => r.DomainEvents);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Collect domain events BEFORE saving so the list isn't affected by the clear below
        var aggregates = ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = aggregates
            .SelectMany(a => a.DomainEvents)
            .ToList();

        // Clear events before Save to prevent double-dispatch on retry
        foreach (var aggregate in aggregates)
            aggregate.ClearDomainEvents();

        var result = await base.SaveChangesAsync(cancellationToken);

        // Publish events AFTER successful save (outbox pattern — simplified)
        foreach (var domainEvent in domainEvents)
            await publisher.Publish(domainEvent, cancellationToken);

        return result;
    }
}
