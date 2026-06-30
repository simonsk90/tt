using TT.Application.Abstractions;

namespace TT.Application.Commands.StartFieldMarking;

/// <summary>
/// Instructs the system to start a field marking operation for the specified robot.
/// If the robot does not yet exist, it will be created with default values.
/// Returns the Robot's ID so the caller can reference it in subsequent queries.
/// </summary>
public sealed record StartFieldMarkingCommand(
    Guid? RobotId = null,
    string RobotName = "TT-Bot-01",
    double BatteryLevel = 85.0) : ICommand<Guid>;
