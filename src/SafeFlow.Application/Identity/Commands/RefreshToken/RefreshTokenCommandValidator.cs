using FluentValidation;

namespace SafeFlow.Application.Identity.Commands.RefreshToken;

/// <summary>
/// Validates <see cref="RefreshTokenCommand"/> before it reaches the handler.
/// All validation messages are in Turkish per <c>PROJECT_RULES.md</c> § 1.
/// </summary>
public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    /// <summary>Initializes the <see cref="RefreshTokenCommandValidator"/> with all rules.</summary>
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshTokenValue)
            .NotEmpty().WithMessage("Yenileme tokeni boş olamaz.");

        RuleFor(x => x.IpAddress)
            .NotEmpty().WithMessage("IP adresi eksik.");
    }
}
