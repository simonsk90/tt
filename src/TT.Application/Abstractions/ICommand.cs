using MediatR;

namespace TT.Application.Abstractions;

/// <summary>
/// Marker interface for commands that return a result.
/// Commands MUST be handled by exactly one <see cref="ICommandHandler{TCommand,TResult}"/>.
/// Commands represent intent to change state — they should not be used for reads.
/// </summary>
public interface ICommand<TResult> : IRequest<TResult>;

/// <summary>Marker interface for void commands.</summary>
public interface ICommand : IRequest;

/// <summary>Strongly-typed handler for commands with a result.</summary>
public interface ICommandHandler<TCommand, TResult> : IRequestHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>;

/// <summary>Strongly-typed handler for void commands.</summary>
public interface ICommandHandler<TCommand> : IRequestHandler<TCommand>
    where TCommand : ICommand;
