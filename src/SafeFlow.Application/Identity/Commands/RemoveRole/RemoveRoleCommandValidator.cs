using FluentValidation;

namespace SafeFlow.Application.Identity.Commands.RemoveRole;

/// <summary>
/// Validates <see cref="RemoveRoleCommand"/> before it reaches the handler.
/// All validation messages are in Turkish per <c>PROJECT_RULES.md</c> § 1.
/// </summary>
public sealed class RemoveRoleCommandValidator : AbstractValidator<RemoveRoleCommand>
{
    /// <summary>Initializes the <see cref="RemoveRoleCommandValidator"/> with all rules.</summary>
    public RemoveRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı kimliği boş olamaz.");

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Rol kimliği boş olamaz.");
    }
}
