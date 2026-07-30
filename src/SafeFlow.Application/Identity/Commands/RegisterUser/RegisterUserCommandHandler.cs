using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.Identity.Interfaces;
using SafeFlow.Domain.Identity.Aggregates;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;
using SafeFlow.SharedKernel.ValueObjects;

namespace SafeFlow.Application.Identity.Commands.RegisterUser;

/// <summary>
/// Handles <see cref="RegisterUserCommand"/>.
/// </summary>
/// <remarks>
/// <para>
/// Flow:
/// <list type="number">
///   <item>Validate uniqueness of the email (silent, no disclosure of existing records).</item>
///   <item>Create Identity user via <see cref="IIdentityService.CreateUserAsync"/>.</item>
///   <item>Build and persist the domain <c>User</c> aggregate.</item>
///   <item>Commit via <see cref="IUnitOfWork"/>.</item>
///   <item>Write an audit log entry.</item>
/// </list>
/// </para>
/// <para>
/// Email verification token generation and dispatch are handled asynchronously by the
/// <c>UserRegisteredSendEmailHandler</c> domain-event handler after the aggregate is saved,
/// keeping this handler cohesive and single-responsibility.
/// </para>
/// </remarks>
public sealed class RegisterUserCommandHandler(
    IIdentityService identityService,
    IRepository<User> userRepository,
    IUnitOfWork unitOfWork,
    IAuditService auditService,
    ILogger<RegisterUserCommandHandler> logger)
    : IRequestHandler<RegisterUserCommand, Result<Guid>>
{
    /// <inheritdoc/>
    public async Task<Result<Guid>> Handle(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing registration for email {Email} on tenant {TenantId}",
            command.Email, command.TenantId);

        // ── 1. Email uniqueness check ────────────────────────────────────────
        bool isUnique = await identityService.IsEmailUniqueAsync(
            command.Email, cancellationToken);

        if (!isUnique)
        {
            // OWASP: Never disclose whether an email already exists.
            // Return a generic validation error, not a "conflict" that confirms existence.
            await auditService.LogAsync(
                AuditAction.Register,
                isSuccess: false,
                ipAddress: command.IpAddress,
                email: command.Email,
                failureReason: "Email already registered",
                userAgent: command.UserAgent,
                cancellationToken: cancellationToken);

            return Result.Failure<Guid>(IdentityErrors.Register.EmailAlreadyExists);
        }

        // ── 2. Create Identity user ─────────────────────────────────────────
        var createResult = await identityService.CreateUserAsync(
            command.Email,
            command.Password,
            command.FirstName,
            command.LastName,
            command.PhoneNumber,
            command.TenantId,
            cancellationToken);

        if (createResult.IsFailure)
        {
            await auditService.LogAsync(
                AuditAction.Register,
                isSuccess: false,
                ipAddress: command.IpAddress,
                email: command.Email,
                failureReason: createResult.Error.Message,
                userAgent: command.UserAgent,
                cancellationToken: cancellationToken);

            return Result.Failure<Guid>(createResult.Error);
        }

        Guid userId = createResult.Value;

        // ── 3. Build domain User aggregate ──────────────────────────────────
        var email = Email.Create(command.Email);
        var fullName = FullName.Create(command.FirstName, command.LastName);
        var phoneNumber = command.PhoneNumber is not null
            ? PhoneNumber.Create(command.PhoneNumber)
            : null;

        var user = User.Create(userId, email, fullName, phoneNumber);
        user.AssignRole(new Guid("00000000-0000-0000-0002-000000000002"));

        await userRepository.AddAsync(user, cancellationToken);

        // ── 4. Commit (dispatches UserRegisteredDomainEvent) ────────────────
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ── 5. Audit ─────────────────────────────────────────────────────────
        await auditService.LogAsync(
            AuditAction.Register,
            isSuccess: true,
            ipAddress: command.IpAddress,
            userId: userId,
            email: command.Email,
            userAgent: command.UserAgent,
            cancellationToken: cancellationToken);

        logger.LogInformation(
            "User {UserId} registered successfully", userId);

        return Result.Success(userId);
    }
}
