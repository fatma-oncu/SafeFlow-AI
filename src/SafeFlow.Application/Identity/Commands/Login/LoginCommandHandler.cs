using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
public sealed partial class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponseDto>>
{
    private readonly IReadRepository<User> _userRepository;
    private readonly IReadRepository<Role> _roleRepository;
    private readonly IRepository<RefreshTokenEntity> _refreshTokenRepository;
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeService? _dateTimeService;
    private readonly ILogger<LoginCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginCommandHandler"/> class.
    /// Preserves the public API of the handler.
    /// </summary>
    public LoginCommandHandler(
        IReadRepository<User> userRepository,
        IReadRepository<Role> roleRepository,
        IRepository<RefreshTokenEntity> refreshTokenRepository,
        IIdentityService identityService,
        IJwtTokenService jwtTokenService,
        IAuditService auditService,
        IUnitOfWork unitOfWork,
        ILogger<LoginCommandHandler> logger)
        : this(userRepository, roleRepository, refreshTokenRepository, identityService, jwtTokenService, auditService, unitOfWork, logger, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginCommandHandler"/> class with <see cref="IDateTimeService"/>.
    /// </summary>
    public LoginCommandHandler(
        IReadRepository<User> userRepository,
        IReadRepository<Role> roleRepository,
        IRepository<RefreshTokenEntity> refreshTokenRepository,
        IIdentityService identityService,
        IJwtTokenService jwtTokenService,
        IAuditService auditService,
        IUnitOfWork unitOfWork,
        ILogger<LoginCommandHandler> logger,
        IDateTimeService? dateTimeService)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dateTimeService = dateTimeService;
    }

    /// <inheritdoc/>
    public async Task<Result<LoginResponseDto>> Handle(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // ── 1. Load user ────────────────────────────────────────────────────
        var spec = new UserByEmailSpecification(command.Email);
        var user = await _userRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (user is null)
        {
            await _auditService.LogAsync(
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
            await _auditService.LogAsync(
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
            await _auditService.LogAsync(
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
        var credResult = await _identityService.ValidateCredentialsAsync(
            command.Email, command.Password, cancellationToken);

        if (credResult.IsFailure)
        {
            // Identity service failure is an exceptional infrastructure condition;
            // return the direct error to caller rather than locking user out.
            return Result.Failure<LoginResponseDto>(credResult.Error);
        }

        if (!credResult.Value)
        {
            // Always lock on any failure — domain method is idempotent (already-locked no-op)
            // Per architecture: lock after failed validation attempt (max attempts managed by Identity lockout config)
            user.Lock("Geçersiz kimlik bilgisi girişi.");
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditService.LogAsync(
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
        var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
        IReadOnlyList<Role> userRoles;

        if (roleIds.Count == 0)
        {
            userRoles = Array.Empty<Role>();
        }
        else
        {
            userRoles = await _roleRepository.ListAsync(
                new RolesByIdsWithPermissionsSpecification(roleIds),
                cancellationToken);
        }

        var roles = userRoles.Select(r => r.Name).ToList();
        var permissions = userRoles
            .SelectMany(r => r.RolePermissions)
            .Select(rp => rp.Permission.CanonicalName)
            .Distinct()
            .ToList();

        string accessToken = _jwtTokenService.GenerateAccessToken(
            user.Id,
            user.Email.Value,
            user.FullName.ToString(),
            tenantId: Guid.Empty, // Tenant enrichment happens in Infrastructure
            roles: roles,
            permissions: permissions);

        string rawRefreshToken = _jwtTokenService.GenerateRefreshToken();
        string tokenHash = _jwtTokenService.HashToken(rawRefreshToken);

        var utcNow = _dateTimeService?.UtcNow ?? DateTime.UtcNow;
        var refreshToken = RefreshTokenEntity.Create(
            id: Guid.NewGuid(),
            userId: user.Id,
            tokenHash: tokenHash,
            familyId: Guid.NewGuid(), // New family for every fresh login
            expiresAt: utcNow.AddDays(_jwtTokenService.RefreshTokenExpirationDays),
            createdByIp: command.IpAddress);

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ── 6. Audit ─────────────────────────────────────────────────────────
        await _auditService.LogAsync(
            AuditAction.Login,
            isSuccess: true,
            ipAddress: command.IpAddress,
            userId: user.Id,
            email: command.Email,
            userAgent: command.UserAgent,
            cancellationToken: cancellationToken);

        LogUserLoggedIn(_logger, user.Id, command.IpAddress);

        var response = new LoginResponseDto
        {
            AccessToken = accessToken,
            ExpiresIn = _jwtTokenService.AccessTokenExpirationMinutes * 60,
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

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "User {UserId} logged in from {IpAddress}")]
    private static partial void LogUserLoggedIn(ILogger logger, Guid userId, string ipAddress);

    private sealed class RolesByIdsWithPermissionsSpecification : BaseSpecification<Role>
    {
        public RolesByIdsWithPermissionsSpecification(IEnumerable<Guid> roleIds)
            : base(r => roleIds.Contains(r.Id))
        {
            AddInclude(r => r.RolePermissions);
            ApplyNoTracking();
        }
    }
}
