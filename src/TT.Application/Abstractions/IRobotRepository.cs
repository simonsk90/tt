using TT.Domain.Robots;

namespace TT.Application.Abstractions;

/// <summary>
/// Repository abstraction for the Robot aggregate.
/// Lives in Application so the domain stays dependency-free.
/// Implemented in TT.Infrastructure using EF Core.
///
/// RULE: Methods must deal only in aggregate roots, not EF entities or DTOs.
/// </summary>
public interface IRobotRepository
{
    Task<Robot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Robot robot, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
