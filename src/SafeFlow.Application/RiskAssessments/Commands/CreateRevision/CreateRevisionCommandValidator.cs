using FluentValidation;

namespace SafeFlow.Application.RiskAssessments.Commands.CreateRevision;

public sealed class CreateRevisionCommandValidator : AbstractValidator<CreateRevisionCommand>
{
    public CreateRevisionCommandValidator()
    {
        RuleFor(x => x.CurrentAssessmentId)
            .NotEmpty().WithMessage("Current assessment ID is required.");

        RuleFor(x => x.CreatedByEmployeeId)
            .NotEmpty().WithMessage("CreatedByEmployeeId is required.");
    }
}
