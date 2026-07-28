using MediatR;
using SafeFlow.Application.Identity.DTOs;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Identity.Queries.GetUserById;

/// <summary>
/// Requests the profile information of the user identified by
/// <see cref="UserId"/>.
/// </summary>
/// <remarks>
/// Intended for administrative scenarios. Authorization is enforced by the
/// application pipeline and/or endpoint policy, not by the query itself.
/// </remarks>
/// <param name="UserId">
/// The unique identifier of the user to retrieve.
/// </param>
public sealed record GetUserByIdQuery(Guid UserId)
    : IRequest<Result<UserDto>>;