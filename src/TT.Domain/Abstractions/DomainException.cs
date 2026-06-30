namespace TT.Domain.Abstractions;

/// <summary>
/// Thrown when a domain invariant is violated.
/// These are business rule violations — not infrastructure errors.
/// Callers should map these to 422 Unprocessable Entity HTTP responses.
/// </summary>
public sealed class DomainException(string message) : Exception(message);
