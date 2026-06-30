using MediatR;

namespace TT.Application.Abstractions;

/// <summary>
/// Marker interface for queries that return a read model.
/// Queries MUST NOT mutate state. Use projections and DTOs — never return domain entities.
/// </summary>
public interface IQuery<TResult> : IRequest<TResult>;

/// <summary>Strongly-typed handler for queries.</summary>
public interface IQueryHandler<TQuery, TResult> : IRequestHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>;
