using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Identity.Commands.RegisterUser;

/// <summary>
/// Centralised, machine-readable error definitions for the Identity bounded context.
/// </summary>
/// <remarks>
/// Error codes follow the convention <c>Domain.Entity.Reason</c>.
/// Human-readable messages are in Turkish per <c>PROJECT_RULES.md</c> § 1.
/// </remarks>
public static class IdentityErrors
{
    /// <summary>Errors raised during user registration.</summary>
    public static class Register
    {
        /// <summary>
        /// The provided email address is already registered.
        /// Returned as a generic validation error to avoid email enumeration (OWASP).
        /// </summary>
        public static readonly Error EmailAlreadyExists = Error.Validation(
            "Identity.Register.EmailAlreadyExists",
            "Bu e-posta adresi zaten kayıtlı.");

        /// <summary>The Identity provider rejected the user-creation request.</summary>
        public static readonly Error CreationFailed = Error.Business(
            "Identity.Register.CreationFailed",
            "Kullanıcı oluşturma işlemi başarısız oldu.");
    }

    /// <summary>Errors raised during login.</summary>
    public static class Login
    {
        /// <summary>
        /// The provided credentials are invalid.
        /// Generic message — does not indicate whether the email or password is wrong.
        /// </summary>
        public static readonly Error InvalidCredentials = Error.Unauthorized(
            "Identity.Login.InvalidCredentials",
            "E-posta adresi veya şifre hatalı.");

        /// <summary>The user account is locked due to repeated failed login attempts.</summary>
        public static readonly Error AccountLocked = Error.Forbidden(
            "Identity.Login.AccountLocked",
            "Hesabınız kilitlenmiştir. Lütfen daha sonra tekrar deneyin veya destek ekibiyle iletişime geçin.");

        /// <summary>The user account is inactive.</summary>
        public static readonly Error AccountInactive = Error.Forbidden(
            "Identity.Login.AccountInactive",
            "Hesabınız aktif değil. Lütfen yöneticinizle iletişime geçin.");

        /// <summary>The user with the given email address was not found.</summary>
        public static readonly Error UserNotFound = Error.Unauthorized(
            "Identity.Login.UserNotFound",
            "E-posta adresi veya şifre hatalı.");
    }

    /// <summary>Errors raised during token refresh.</summary>
    public static class RefreshToken
    {
        /// <summary>The provided refresh token does not exist in the store.</summary>
        public static readonly Error NotFound = Error.Unauthorized(
            "Identity.RefreshToken.NotFound",
            "Geçersiz yenileme tokeni.");

        /// <summary>The refresh token has expired.</summary>
        public static readonly Error Expired = Error.Unauthorized(
            "Identity.RefreshToken.Expired",
            "Yenileme tokeninin süresi dolmuştur. Lütfen tekrar giriş yapın.");

        /// <summary>A revoked token was presented — possible token theft detected.</summary>
        public static readonly Error StolenTokenDetected = Error.Unauthorized(
            "Identity.RefreshToken.StolenTokenDetected",
            "Güvenlik ihlali tespit edildi. Tüm oturumlarınız sonlandırıldı. Lütfen tekrar giriş yapın.");
    }

    /// <summary>Errors raised during logout.</summary>
    public static class Logout
    {
        /// <summary>The token to revoke was not found.</summary>
        public static readonly Error TokenNotFound = Error.NotFound(
            "Identity.Logout.TokenNotFound",
            "Geçersiz oturum tokeni.");
    }

    /// <summary>Errors raised during password operations.</summary>
    public static class Password
    {
        /// <summary>The current password provided for verification is incorrect.</summary>
        public static readonly Error CurrentPasswordInvalid = Error.Validation(
            "Identity.Password.CurrentPasswordInvalid",
            "Mevcut şifreniz hatalı.");

        /// <summary>The password reset token is invalid or has expired.</summary>
        public static readonly Error ResetTokenInvalid = Error.Validation(
            "Identity.Password.ResetTokenInvalid",
            "Şifre sıfırlama bağlantısı geçersiz veya süresi dolmuş.");

        /// <summary>The password reset operation failed in the Identity provider.</summary>
        public static readonly Error ResetFailed = Error.Business(
            "Identity.Password.ResetFailed",
            "Şifre sıfırlama işlemi başarısız oldu.");

        /// <summary>The password change operation failed in the Identity provider.</summary>
        public static readonly Error ChangeFailed = Error.Business(
            "Identity.Password.ChangeFailed",
            "Şifre değiştirme işlemi başarısız oldu.");
    }

    /// <summary>Errors raised during user management operations.</summary>
    public static class User
    {
        /// <summary>The requested user could not be found.</summary>
        public static readonly Error NotFound = Error.NotFound(
            "Identity.User.NotFound",
            "Kullanıcı bulunamadı.");

        /// <summary>The requesting user is not authenticated.</summary>
        public static readonly Error NotAuthenticated = Error.Unauthorized(
            "Identity.User.NotAuthenticated",
            "Bu işlem için oturum açmanız gerekmektedir.");
    }

    /// <summary>Errors raised during role management operations.</summary>
    public static class Role
    {
        /// <summary>The requested role could not be found.</summary>
        public static readonly Error NotFound = Error.NotFound(
            "Identity.Role.NotFound",
            "Rol bulunamadı.");
    }
}
