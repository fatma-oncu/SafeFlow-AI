using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.Identity.Interfaces;
using SafeFlow.Infrastructure.Identity;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Infrastructure.Services;

/// <summary>
/// ASP.NET Core Identity-backed implementation of <see cref="IIdentityService"/>.
/// </summary>
/// <remarks>
/// <para>
/// This class is the sole bridge between the Application layer and ASP.NET Core Identity.
/// It translates between the Application's <see cref="Result{T}"/> contract and
/// Identity's <see cref="IdentityResult"/> / <see cref="SignInResult"/> types.
/// </para>
/// <para>
/// No domain types are imported here — the service operates exclusively with primitive
/// values (Guid, string) and returns <see cref="Result"/> / <see cref="Result{T}"/>.
/// </para>
/// </remarks>
internal sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<IdentityService> _logger;

    /// <summary>
    /// Initialises a new <see cref="IdentityService"/>.
    /// </summary>
    public IdentityService(
        UserManager<ApplicationUser> userManager,
        ILogger<IdentityService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<Guid>> CreateUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string? phoneNumber,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email.ToLowerInvariant().Trim(),
            Email = email.ToLowerInvariant().Trim(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            PhoneNumber = phoneNumber,
            TenantId = tenantId,
            EmailConfirmed = false,
        };

        var identityResult = await _userManager.CreateAsync(user, password);

        if (!identityResult.Succeeded)
        {
            var errors = string.Join("; ", identityResult.Errors.Select(e => e.Description));

            _logger.LogWarning(
                "Identity user creation failed for {Email}: {Errors}", email, errors);

            return Result.Failure<Guid>(
                Error.Business("Identity.CreateUser.Failed", errors));
        }

        return Result.Success(user.Id);
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email.ToLowerInvariant().Trim());

        if (user is null)
        {
            return Result.Success(false);
        }

        bool isValid = await _userManager.CheckPasswordAsync(user, password);
        return Result.Success(isValid);
    }

    /// <inheritdoc/>
    public async Task<Result> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return Result.Failure(Error.NotFound("Identity.User.NotFound", "Kullanıcı bulunamadı."));
        }

        var identityResult = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        if (!identityResult.Succeeded)
        {
            var errors = string.Join("; ", identityResult.Errors.Select(e => e.Description));
            return Result.Failure(Error.Business("Identity.Password.ChangeFailed", errors));
        }

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result<string>> GeneratePasswordResetTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return Result.Failure<string>(
                Error.NotFound("Identity.User.NotFound", "Kullanıcı bulunamadı."));
        }

        string token = await _userManager.GeneratePasswordResetTokenAsync(user);
        return Result.Success(token);
    }

    /// <inheritdoc/>
    public async Task<Result> ResetPasswordAsync(
        Guid userId,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return Result.Failure(Error.NotFound("Identity.User.NotFound", "Kullanıcı bulunamadı."));
        }

        var identityResult = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (!identityResult.Succeeded)
        {
            var errors = string.Join("; ", identityResult.Errors.Select(e => e.Description));
            return Result.Failure(Error.Business("Identity.Password.ResetFailed", errors));
        }

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<bool> IsEmailUniqueAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var existing = await _userManager.FindByEmailAsync(email.ToLowerInvariant().Trim());
        return existing is null;
    }
}
