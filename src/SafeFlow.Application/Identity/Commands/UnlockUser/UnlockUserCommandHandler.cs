using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.Identity.Commands.RegisterUser;
using SafeFlow.Application.Identity.Interfaces;
using SafeFlow.Domain.Identity.Aggregates;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Identity.Commands.UnlockUser;

/// <summary>
/// Handles <see cref="UnlockUserCommand"/> — unlocks the target user's account
/// via the domain aggregate and writes an audit entry.
/// </summary>
public sealed class UnlockUserCommandHandler(
    IRepository<User> userRepository,
    IAuditService auditService,
    IUnitOfWork unitOfWork,
    ILogger<UnlockUserCommandHandler> logger)
    : IRequestHandler<UnlockUserCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(
        UnlockUserCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(IdentityErrors.User.NotFound);
        }

        user.Unlock();

        userRepository.Update(user);

        await auditService.LogAsync(
            AuditAction.UserUnlocked,
            isSuccess: true,
            ipAddress: string.Empty,
            userId: user.Id,
            email: user.Email.Value,
            cancellationToken: cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User {UserId} unlocked.", command.UserId);

        return Result.Success();
    }
}
