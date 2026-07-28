using Mapster;
using MediatR;
using SafeFlow.Application.Identity.DTOs;
using SafeFlow.Domain.Identity.Aggregates;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Identity.Queries.GetRoles;

/// <summary>
/// Handles <see cref="GetRolesQuery"/> by retrieving all roles and mapping them to <see cref="RoleDto"/> instances.
/// </summary>
public sealed class GetRolesQueryHandler(
    IReadRepository<Role> roleRepository)
    : IRequestHandler<GetRolesQuery, Result<IReadOnlyList<RoleDto>>>
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<RoleDto>>> Handle(
        GetRolesQuery request,
        CancellationToken cancellationToken)
    {
        var roles = await roleRepository.ListAsync(cancellationToken);

        var roleDtos = roles.Adapt<IReadOnlyList<RoleDto>>();

        return Result.Success(roleDtos);
    }
}