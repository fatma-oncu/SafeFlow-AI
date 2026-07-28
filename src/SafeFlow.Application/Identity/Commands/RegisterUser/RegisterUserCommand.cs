using MediatR;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Identity.Commands.RegisterUser;

/// <summary>
/// Initiates the user-registration flow: creates an Identity record, seeds the
/// domain <c>User</c> aggregate, and dispatches a verification email.
/// </summary>
/// <param name="Email">The prospective user's email address.</param>
/// <param name="Password">The raw (unhashed) password chosen by the user.</param>
/// <param name="FirstName">The user's given name.</param>
/// <param name="LastName">The user's family name.</param>
/// <param name="PhoneNumber">Optional phone number.</param>
/// <param name="TenantId">The tenant (company) this user registers under.</param>
/// <param name="IpAddress">The client IP address, for audit logging.</param>
/// <param name="UserAgent">The client User-Agent header, for audit logging.</param>
public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    Guid TenantId,
    string IpAddress,
    string? UserAgent) : IRequest<Result<Guid>>;
