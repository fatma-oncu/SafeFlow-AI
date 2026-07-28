using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.Identity.Commands.RegisterUser;
using SafeFlow.Application.Identity.Interfaces;
using SafeFlow.Domain.Identity.Aggregates;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Identity.Commands.RemoveRole;

/// <summary>
/// Handles <see cref="RemoveRoleCommand"/> — removes the given role from the user
/// through the domain aggregate and writes an audit entry.
/// </summary>
public sealed class RemoveRoleCommandHandler(
    IRepository<User> userRepository,
    IAuditService auditService,
    IUnitOfWork unitOfWork,
    ILogger<RemoveRoleCommandHandler> logger)
    : IRequestHandler<RemoveRoleCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(
        RemoveRoleCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(IdentityErrors.User.NotFound);
        }

        user.RemoveRole(command.RoleId);

        userRepository.Update(user);

        await auditService.LogAsync(
            AuditAction.RoleRemoved,
            isSuccess: true,
            ipAddress: string.Empty,
            userId: user.Id,
            email: user.Email.Value,
            cancellationToken: cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Role {RoleId} removed from user {UserId}", command.RoleId, command.UserId);

        return Result.Success();
    }
}
