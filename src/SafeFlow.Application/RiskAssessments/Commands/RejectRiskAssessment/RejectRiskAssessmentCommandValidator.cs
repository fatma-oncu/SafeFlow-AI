using FluentValidation;

namespace SafeFlow.Application.RiskAssessments.Commands.RejectRiskAssessment;

public sealed class RejectRiskAssessmentCommandValidator : AbstractValidator<RejectRiskAssessmentCommand>
{
    public RejectRiskAssessmentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Risk assessment ID is required.");

        RuleFor(x => x.ReviewerEmployeeId)
            .NotEmpty().WithMessage("ReviewerEmployeeId is required.");

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Rejection comment is required.")
            .MaximumLength(1000).WithMessage("Rejection comment must not exceed 1000 characters.");
    }
}
