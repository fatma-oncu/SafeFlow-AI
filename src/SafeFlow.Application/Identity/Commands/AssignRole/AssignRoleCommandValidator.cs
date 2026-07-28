using FluentValidation;

namespace SafeFlow.Application.Identity.Commands.AssignRole;

/// <summary>
/// Validates <see cref="AssignRoleCommand"/> before it reaches the handler.
/// All validation messages are in Turkish per <c>PROJECT_RULES.md</c> § 1.
/// </summary>
public sealed class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
{
    /// <summary>Initializes the <see cref="AssignRoleCommandValidator"/> with all rules.</summary>
    public AssignRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı kimliği boş olamaz.");

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Rol kimliği boş olamaz.");
    }
}
