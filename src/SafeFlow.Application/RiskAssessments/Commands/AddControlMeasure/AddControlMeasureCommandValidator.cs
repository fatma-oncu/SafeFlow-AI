using FluentValidation;

namespace SafeFlow.Application.RiskAssessments.Commands.AddControlMeasure;

public sealed class AddControlMeasureCommandValidator : AbstractValidator<AddControlMeasureCommand>
{
    public AddControlMeasureCommandValidator()
    {
        RuleFor(x => x.RiskAssessmentId)
            .NotEmpty().WithMessage("Risk assessment ID is required.");

        RuleFor(x => x.HazardId)
            .NotEmpty().WithMessage("Hazard ID is required.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Control description is required.")
            .MaximumLength(500).WithMessage("Control description must not exceed 500 characters.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid control measure type.");
    }
}
