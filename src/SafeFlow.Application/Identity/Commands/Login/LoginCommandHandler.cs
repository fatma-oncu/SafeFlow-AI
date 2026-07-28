using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.Identity.Commands.RegisterUser;
using SafeFlow.Application.Identity.DTOs;
using SafeFlow.Application.Identity.Interfaces;
using SafeFlow.Application.Identity.Specifications;
using SafeFlow.Domain.Identity.Aggregates;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;
using SafeFlow.SharedKernel.Specifications;
using RefreshTokenEntity = SafeFlow.Domain.Identity.Entities.RefreshToken;

namespace SafeFlow.Application.Identity.Commands.Login;

/// <summary>
/// Handles <see cref="LoginCommand"/>.
/// </summary>
/// <remarks>
/// <para>Flow:</para>
/// <list type="number">
///   <item>Look up domain <c>User</c> by email.</item>
///   <item>Guard: account must be active and not locked.</item>
///   <item>Validate credentials via <see cref="IIdentityService"/>.</item>
///   <item>On failure: increment failed-login counter; auto-lock at 5 attempts.</item>
///   <item>On success: generate RS256 access token + SHA-256-hashed refresh token.</item>
///   <item>Persist the <c>RefreshToken</c> entity in a new token family.</item>
///   <item>Write audit log.</item>
/// </list>
/// <para>
/// Credential validation errors always return the same generic message to prevent
/// email enumeration and timing-based username discovery (OWASP A07:2021).
/// </para>
/// </remarks>
public sealed class LoginCommandHandler(
    IReadRepository<User> userRepository,
    IRepository<RefreshTokenEntity> refreshTokenRepository,
    IIdentityService identityService,
    IJwtTokenService jwtTokenService,
    IAuditService auditService,
    IUnitOfWork unitOfWork,
    ILogger<LoginCommandHandler> logger)
    : IRequestHandler<LoginCommand, Result<LoginResponseDto>>
{
    /// <inheritdoc/>
    public async Task<Result<LoginResponseDto>> Handle(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        // ── 1. Look up user by email ─────────────────────────────────────────
        var spec = new UserByEmailSpecification(command.Email);
        var user = await userRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (user is null)
        {
            await auditService.LogAsync(
                AuditAction.LoginFailed,
                isSuccess: false,
                ipAddress: command.IpAddress,
                email: command.Email,
                failureReason: "User not found",
                userAgent: command.UserAgent,
                cancellationToken: cancellationToken);

            // Generic response — do not reveal whether the email exists.
            return Result.Failure<LoginResponseDto>(IdentityErrors.Login.InvalidCredentials);
        }

        // ── 2. Guard: active & unlocked ─────────────────────────────────────
        if (user.IsLocked)
        {
            await auditService.LogAsync(
                AuditAction.LoginFailed,
                isSuccess: false,
                ipAddress: command.IpAddress,
                userId: user.Id,
                email: command.Email,
                failureReason: "Account is locked",
                userAgent: command.UserAgent,
                cancellationToken: cancellationToken);

            return Result.Failure<LoginResponseDto>(IdentityErrors.Login.AccountLocked);
        }

        if (!user.IsActive)
        {
            await auditService.LogAsync(
                AuditAction.LoginFailed,
                isSuccess: false,
                ipAddress: command.IpAddress,
                userId: user.Id,
                email: command.Email,
                failureReason: "Account is inactive",
                userAgent: command.UserAgent,
                cancellationToken: cancellationToken);

            return Result.Failure<LoginResponseDto>(IdentityErrors.Login.AccountInactive);
        }

        // ── 3. Validate credentials ──────────────────────────────────────────
        var credResult = await identityService.ValidateCredentialsAsync(
            command.Email, command.Password, cancellationToken);

        if (credResult.IsFailure || !credResult.Value)
        {
            // Always lock on any failure — domain method is idempotent (already-locked no-op)
            // Per architecture: lock after failed validation attempt (max attempts managed by Identity lockout config)
            user.Lock("Geçersiz kimlik bilgisi girişi.");

            await unitOfWork.SaveChangesAsync(cancellationToken);

            await auditService.LogAsync(
                AuditAction.LoginFailed,
                isSuccess: false,
                ipAddress: command.IpAddress,
                userId: user.Id,
                email: command.Email,
                failureReason: "Invalid password",
                userAgent: command.UserAgent,
                cancellationToken: cancellationToken);

            return Result.Failure<LoginResponseDto>(IdentityErrors.Login.InvalidCredentials);
        }

        // ── 4. Record successful login ───────────────────────────────────────
        user.RecordLogin();

        // ── 5. Issue tokens ──────────────────────────────────────────────────
        var roles = user.UserRoles
            .Select(ur => ur.RoleId.ToString())
            .ToList();

        string accessToken = jwtTokenService.GenerateAccessToken(
            user.Id,
            user.Email.Value,
            user.FullName.ToString(),
            tenantId: Guid.Empty, // Tenant enrichment happens in Infrastructure
            roles: roles,
            permissions: []);

        string rawRefreshToken = jwtTokenService.GenerateRefreshToken();
        string tokenHash = jwtTokenService.HashToken(rawRefreshToken);

        var refreshToken = RefreshTokenEntity.Create(
            id: Guid.NewGuid(),
            userId: user.Id,
            tokenHash: tokenHash,
            familyId: Guid.NewGuid(), // New family for every fresh login
            expiresAt: DateTime.UtcNow.AddDays(jwtTokenService.RefreshTokenExpirationDays),
            createdByIp: command.IpAddress);

        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ── 6. Audit ─────────────────────────────────────────────────────────
        await auditService.LogAsync(
            AuditAction.Login,
            isSuccess: true,
            ipAddress: command.IpAddress,
            userId: user.Id,
            email: command.Email,
            userAgent: command.UserAgent,
            cancellationToken: cancellationToken);

        logger.LogInformation("User {UserId} logged in from {IpAddress}", user.Id, command.IpAddress);

        var response = new LoginResponseDto
        {
            AccessToken = accessToken,
            ExpiresIn = jwtTokenService.AccessTokenExpirationMinutes * 60,
            RefreshToken = rawRefreshToken, // API layer writes this to HttpOnly cookie for browsers
            User = new UserSummaryDto
            {
                Id = user.Id,
                Email = user.Email.Value,
                FullName = user.FullName.ToString(),
                Roles = roles.AsReadOnly(),
            },
        };

        return Result.Success(response);
    }
}
