using FluentValidation;

namespace SafeFlow.Application.RiskAssessments.Commands.UpdateRiskAssessment;

public sealed class UpdateRiskAssessmentCommandValidator : AbstractValidator<UpdateRiskAssessmentCommand>
{
    public UpdateRiskAssessmentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Risk assessment ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("Department ID is required.");

        RuleFor(x => x.ResponsibleEmployeeId)
            .NotEmpty().WithMessage("ResponsibleEmployeeId is required.");
    }
}
