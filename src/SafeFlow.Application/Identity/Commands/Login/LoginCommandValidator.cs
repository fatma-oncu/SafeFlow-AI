using FluentValidation;

namespace SafeFlow.Application.Identity.Commands.Login;

/// <summary>
/// Validates <see cref="LoginCommand"/> before it reaches the handler.
/// All validation messages are in Turkish per <c>PROJECT_RULES.md</c> § 1.
/// </summary>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    /// <summary>Initializes the <see cref="LoginCommandValidator"/> with all rules.</summary>
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi boş olamaz.")
            .MaximumLength(254).WithMessage("E-posta adresi 254 karakterden uzun olamaz.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre boş olamaz.")
            .MaximumLength(128).WithMessage("Şifre 128 karakterden uzun olamaz.");

        RuleFor(x => x.IpAddress)
            .NotEmpty().WithMessage("IP adresi eksik.");
    }
}
