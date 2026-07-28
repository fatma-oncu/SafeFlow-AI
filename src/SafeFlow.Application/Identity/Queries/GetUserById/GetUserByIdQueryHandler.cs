using Mapster;
using MediatR;
using SafeFlow.Application.Identity.Commands.RegisterUser;
using SafeFlow.Application.Identity.DTOs;
using SafeFlow.Application.Identity.Specifications;
using SafeFlow.Domain.Identity.Aggregates;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Identity.Queries.GetUserById;

/// <summary>
/// Handles <see cref="GetUserByIdQuery"/> by retrieving a user and mapping it to <see cref="UserDto"/>.
/// </summary>
public sealed class GetUserByIdQueryHandler(
    IReadRepository<User> userRepository)
    : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    /// <inheritdoc />
    public async Task<Result<UserDto>> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        var specification = new UserByIdWithRolesSpecification(request.UserId);

        var user = await userRepository.FirstOrDefaultAsync(
            specification,
            cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserDto>(IdentityErrors.User.NotFound);
        }

        var userDto = user.Adapt<UserDto>();

        return Result.Success(userDto);
    }
}