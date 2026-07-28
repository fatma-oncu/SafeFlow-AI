using MediatR;
using SafeFlow.Application.Identity.DTOs;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Identity.Queries.GetRoles;

/// <summary>
/// Represents a query that retrieves all roles defined in the system.
/// </summary>
/// <remarks>
/// Intended for administrative scenarios where the complete role list is required.
/// If the number of roles grows significantly, consider introducing pagination.
/// </remarks>
public sealed record GetRolesQuery
    : IRequest<Result<IReadOnlyList<RoleDto>>>;