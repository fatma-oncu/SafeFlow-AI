using FluentValidation;

namespace SafeFlow.Application.Identity.Commands.ChangePassword;

/// <summary>
/// Validates <see cref="ChangePasswordCommand"/> before it reaches the handler.
/// All validation messages are in Turkish per <c>PROJECT_RULES.md</c>.
/// </summary>
public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChangePasswordCommandValidator"/> class.
    /// </summary>
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage("Mevcut şifre boş olamaz.");

        RuleFor(x => x.NewPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Yeni şifre boş olamaz.")
            .MinimumLength(8).WithMessage("Yeni şifre en az 8 karakter olmalıdır.")
            .MaximumLength(128).WithMessage("Yeni şifre 128 karakterden uzun olamaz.")
            .Matches(@"[A-Z]").WithMessage("Yeni şifre en az bir büyük harf içermelidir.")
            .Matches(@"[a-z]").WithMessage("Yeni şifre en az bir küçük harf içermelidir.")
            .Matches(@"\d").WithMessage("Yeni şifre en az bir rakam içermelidir.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Yeni şifre en az bir özel karakter içermelidir.")
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("Yeni şifre mevcut şifreyle aynı olamaz.");

        RuleFor(x => x.IpAddress)
            .NotEmpty()
            .WithMessage("IP adresi boş olamaz.");
    }
}