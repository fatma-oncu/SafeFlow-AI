using MediatR;
using SafeFlow.Application.Identity.DTOs;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Identity.Queries.GetCurrentUser;

/// <summary>
/// Represents a query that retrieves the profile of the currently authenticated user.
/// </summary>
/// <remarks>
/// The authenticated user is resolved inside the handler through
/// <see cref="SafeFlow.SharedKernel.Interfaces.ICurrentUserService"/>.
/// No caller-supplied user identifier is accepted, which helps prevent
/// insecure direct object reference (IDOR) vulnerabilities.
/// </remarks>
public sealed record GetCurrentUserQuery
    : IRequest<Result<UserDto>>;