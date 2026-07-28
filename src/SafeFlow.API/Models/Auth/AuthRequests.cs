namespace SafeFlow.API.Models.Auth;

/// <summary>Request body for <c>POST /api/v1/auth/register</c>.</summary>
public sealed record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    Guid TenantId);

/// <summary>Request body for <c>POST /api/v1/auth/login</c>.</summary>
public sealed record LoginRequest(
    string Email,
    string Password);

/// <summary>Request body for <c>POST /api/v1/auth/refresh</c>.</summary>
public sealed record RefreshTokenRequest(
    string RefreshToken);

/// <summary>Request body for <c>POST /api/v1/auth/logout</c>.</summary>
public sealed record LogoutRequest(
    string RefreshToken);

/// <summary>Request body for <c>POST /api/v1/auth/change-password</c>.</summary>
public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);

/// <summary>Request body for <c>POST /api/v1/auth/forgot-password</c>.</summary>
public sealed record ForgotPasswordRequest(
    string Email);

/// <summary>Request body for <c>POST /api/v1/auth/reset-password</c>.</summary>
public sealed record ResetPasswordRequest(
    Guid UserId,
    string Token,
    string NewPassword);
