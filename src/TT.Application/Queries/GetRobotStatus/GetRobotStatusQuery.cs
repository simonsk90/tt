using TT.Application.Abstractions;
using TT.Domain.Robots;

namespace TT.Application.Queries.GetRobotStatus;

/// <summary>
/// Returns a lightweight read model for the current status of a robot.
/// Queries MUST NOT return domain entities — only DTOs.
/// </summary>
public sealed record GetRobotStatusQuery(Guid RobotId) : IQuery<RobotStatusDto?>;
