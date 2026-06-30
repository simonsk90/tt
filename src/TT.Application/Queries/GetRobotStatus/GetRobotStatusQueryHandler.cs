using TT.Application.Abstractions;

namespace TT.Application.Queries.GetRobotStatus;

public sealed class GetRobotStatusQueryHandler(IRobotRepository repository)
    : IQueryHandler<GetRobotStatusQuery, RobotStatusDto?>
{
    public async Task<RobotStatusDto?> Handle(GetRobotStatusQuery query, CancellationToken cancellationToken)
    {
        var robot = await repository.GetByIdAsync(query.RobotId, cancellationToken);
        return robot is null ? null : RobotStatusDto.From(robot);
    }
}
