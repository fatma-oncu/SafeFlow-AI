using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.Identity.Commands.RegisterUser;
using SafeFlow.Application.Identity.Interfaces;
using SafeFlow.Domain.Identity.Aggregates;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Identity.Commands.LockUser;

/// <summary>
/// Handles <see cref="LockUserCommand"/> — locks the target user's account via the
/// domain aggregate and writes an audit entry.
/// </summary>
public sealed class LockUserCommandHandler(
    IRepository<User> userRepository,
    IAuditService auditService,
    IUnitOfWork unitOfWork,
    ILogger<LockUserCommandHandler> logger)
    : IRequestHandler<LockUserCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(
        LockUserCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(IdentityErrors.User.NotFound);
        }

        user.Lock(command.Reason);

        userRepository.Update(user);

        await auditService.LogAsync(
            AuditAction.UserLocked,
            isSuccess: true,
            ipAddress: string.Empty,
            userId: user.Id,
            email: user.Email.Value,
            failureReason: command.Reason,
            cancellationToken: cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "User {UserId} locked. Reason: {Reason}", command.UserId, command.Reason);

        return Result.Success();
    }
}
