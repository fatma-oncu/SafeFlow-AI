using FluentValidation;

namespace SafeFlow.Application.Identity.Commands.ResetPassword;

/// <summary>
/// Validates <see cref="ResetPasswordCommand"/> before it reaches the handler.
/// All validation messages are in Turkish per <c>PROJECT_RULES.md</c>.
/// </summary>
public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    /// <summary>
    /// Initializes validation rules for <see cref="ResetPasswordCommand"/>.
    /// </summary>
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı kimliği boş olamaz.");

        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Şifre sıfırlama belirteci boş olamaz.")
            .MaximumLength(512)
            .WithMessage("Şifre sıfırlama belirteci geçersiz.");

        RuleFor(x => x.NewPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage("Yeni şifre boş olamaz.")
            .MinimumLength(8)
                .WithMessage("Yeni şifre en az 8 karakter olmalıdır.")
            .MaximumLength(128)
                .WithMessage("Yeni şifre 128 karakterden uzun olamaz.")
            .Matches("[A-Z]")
                .WithMessage("Yeni şifre en az bir büyük harf içermelidir.")
            .Matches("[a-z]")
                .WithMessage("Yeni şifre en az bir küçük harf içermelidir.")
            .Matches(@"\d")
                .WithMessage("Yeni şifre en az bir rakam içermelidir.")
            .Matches(@"[^a-zA-Z0-9]")
                .WithMessage("Yeni şifre en az bir özel karakter içermelidir.");

        RuleFor(x => x.IpAddress)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage("IP adresi boş olamaz.")
            .MaximumLength(45)
                .WithMessage("Geçersiz IP adresi.")
            .Must(static ip => Uri.CheckHostName(ip) != UriHostNameType.Unknown)
                .WithMessage("Geçersiz IP adresi.");
    }
}