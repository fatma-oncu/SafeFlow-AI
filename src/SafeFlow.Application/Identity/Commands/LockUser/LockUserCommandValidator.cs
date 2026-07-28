using FluentValidation;

namespace SafeFlow.Application.Identity.Commands.LockUser;

/// <summary>
/// Validates <see cref="LockUserCommand"/> before it reaches the handler.
/// All validation messages are in Turkish per <c>PROJECT_RULES.md</c> § 1.
/// </summary>
public sealed class LockUserCommandValidator : AbstractValidator<LockUserCommand>
{
    /// <summary>Initializes the <see cref="LockUserCommandValidator"/> with all rules.</summary>
    public LockUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı kimliği boş olamaz.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Kilit nedeni boş olamaz.")
            .MaximumLength(500).WithMessage("Kilit nedeni 500 karakterden uzun olamaz.");
    }
}
