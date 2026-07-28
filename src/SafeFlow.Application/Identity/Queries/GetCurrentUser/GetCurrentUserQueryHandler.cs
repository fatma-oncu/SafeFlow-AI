using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.Identity.Commands.RegisterUser;
using SafeFlow.Application.Identity.DTOs;
using SafeFlow.Application.Identity.Specifications;
using SafeFlow.Domain.Identity.Aggregates;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Identity.Queries.GetCurrentUser;

/// <summary>
/// Handles <see cref="GetCurrentUserQuery"/> by retrieving the authenticated user
/// and mapping it to a <see cref="UserDto"/>.
/// </summary>
public sealed class GetCurrentUserQueryHandler(
    ICurrentUserService currentUserService,
    IReadRepository<User> userRepository,
    ILogger<GetCurrentUserQueryHandler> logger)
    : IRequestHandler<GetCurrentUserQuery, Result<UserDto>>
{
    /// <inheritdoc />
    public async Task<Result<UserDto>> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || currentUserService.UserId is null)
        {
            return Result.Failure<UserDto>(IdentityErrors.User.NotAuthenticated);
        }

        var specification = new UserByIdWithRolesSpecification(currentUserService.UserId.Value);

        var user = await userRepository.FirstOrDefaultAsync(
            specification,
            cancellationToken);

        if (user is null)
        {
            logger.LogWarning(
                "Authenticated user {UserId} was not found in the domain store.",
                currentUserService.UserId.Value);

            return Result.Failure<UserDto>(IdentityErrors.User.NotFound);
        }

        var userDto = user.Adapt<UserDto>();

        return Result.Success(userDto);
    }
}