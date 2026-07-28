using FluentValidation;

namespace SafeFlow.Application.Identity.Commands.RegisterUser;

/// <summary>
/// Validates <see cref="RegisterUserCommand"/> before it reaches the handler.
/// All validation messages are in Turkish per <c>PROJECT_RULES.md</c> § 1.
/// </summary>
public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    /// <summary>Initializes the <see cref="RegisterUserCommandValidator"/> with all rules.</summary>
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi boş olamaz.")
            .MaximumLength(254).WithMessage("E-posta adresi 254 karakterden uzun olamaz.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre boş olamaz.")
            .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır.")
            .MaximumLength(128).WithMessage("Şifre 128 karakterden uzun olamaz.")
            .Matches(@"[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir.")
            .Matches(@"[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir.")
            .Matches(@"\d").WithMessage("Şifre en az bir rakam içermelidir.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Şifre en az bir özel karakter içermelidir.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ad alanı boş olamaz.")
            .MaximumLength(100).WithMessage("Ad 100 karakterden uzun olamaz.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyad alanı boş olamaz.")
            .MaximumLength(100).WithMessage("Soyad 100 karakterden uzun olamaz.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Telefon numarası 20 karakterden uzun olamaz.")
            .When(x => x.PhoneNumber is not null);

        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Geçerli bir şirket kimliği girilmelidir.");

        RuleFor(x => x.IpAddress)
            .NotEmpty().WithMessage("IP adresi eksik.");
    }
}
