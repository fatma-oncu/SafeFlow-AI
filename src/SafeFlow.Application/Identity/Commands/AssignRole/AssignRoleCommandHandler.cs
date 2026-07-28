using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.Identity.Commands.RegisterUser;
using SafeFlow.Application.Identity.Interfaces;
using SafeFlow.Domain.Identity.Aggregates;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Identity.Commands.AssignRole;

/// <summary>
/// Handles <see cref="AssignRoleCommand"/> — assigns the given role to the user
/// through the domain aggregate and writes an audit entry.
/// </summary>
public sealed class AssignRoleCommandHandler(
    IRepository<User> userRepository,
    IReadRepository<Role> roleRepository,
    IAuditService auditService,
    IUnitOfWork unitOfWork,
    ILogger<AssignRoleCommandHandler> logger)
    : IRequestHandler<AssignRoleCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(
        AssignRoleCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(IdentityErrors.User.NotFound);
        }

        var role = await roleRepository.GetByIdAsync(command.RoleId, cancellationToken);

        if (role is null)
        {
            return Result.Failure(IdentityErrors.Role.NotFound);
        }

        user.AssignRole(command.RoleId);

        userRepository.Update(user);

        await auditService.LogAsync(
            AuditAction.RoleAssigned,
            isSuccess: true,
            ipAddress: string.Empty,
            userId: user.Id,
            email: user.Email.Value,
            cancellationToken: cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Role {RoleId} assigned to user {UserId}", command.RoleId, command.UserId);

        return Result.Success();
    }
}
