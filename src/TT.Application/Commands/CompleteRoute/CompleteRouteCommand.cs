using TT.Application.Abstractions;

namespace TT.Application.Commands.CompleteRoute;

/// <summary>
/// Instructs the system to complete the active route for a given robot.
/// Typically enqueued by the <c>CompleteRouteJob</c> Hangfire job — not called directly by the API.
/// </summary>
public sealed record CompleteRouteCommand(Guid RobotId) : ICommand;
