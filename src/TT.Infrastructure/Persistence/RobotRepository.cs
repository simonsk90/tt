using Microsoft.EntityFrameworkCore;
using TT.Application.Abstractions;
using TT.Domain.Robots;
using TT.Infrastructure.Persistence;

namespace TT.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of <see cref="IRobotRepository"/>.
/// Wraps AppDbContext — callers never interact with DbContext directly.
/// </summary>
public sealed class RobotRepository(AppDbContext dbContext) : IRobotRepository
{
    public async Task<Robot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Robots.FindAsync([id], cancellationToken);

    public async Task AddAsync(Robot robot, CancellationToken cancellationToken = default) =>
        await dbContext.Robots.AddAsync(robot, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
