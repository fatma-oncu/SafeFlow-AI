using FluentValidation;

namespace SafeFlow.Application.RiskAssessments.Commands.CreateRiskAssessment;

public sealed class CreateRiskAssessmentCommandValidator : AbstractValidator<CreateRiskAssessmentCommand>
{
    public CreateRiskAssessmentCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("Department ID is required.");

        RuleFor(x => x.CreatedByEmployeeId)
            .NotEmpty().WithMessage("CreatedByEmployeeId is required.");

        RuleFor(x => x.ResponsibleEmployeeId)
            .NotEmpty().WithMessage("ResponsibleEmployeeId is required.");

        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");
    }
}
