using FluentValidation;

namespace SafeFlow.Application.RiskAssessments.Commands.AddHazard;

public sealed class AddHazardCommandValidator : AbstractValidator<AddHazardCommand>
{
    public AddHazardCommandValidator()
    {
        RuleFor(x => x.RiskAssessmentId)
            .NotEmpty().WithMessage("Risk assessment ID is required.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Hazard description is required.")
            .MaximumLength(500).WithMessage("Hazard description must not exceed 500 characters.");

        RuleFor(x => x.InitialLikelihood)
            .IsInEnum().WithMessage("Invalid initial likelihood rating.");

        RuleFor(x => x.InitialSeverity)
            .IsInEnum().WithMessage("Invalid initial severity rating.");

        RuleFor(x => x.ResidualLikelihood)
            .IsInEnum().WithMessage("Invalid residual likelihood rating.");

        RuleFor(x => x.ResidualSeverity)
            .IsInEnum().WithMessage("Invalid residual severity rating.");
    }
}
