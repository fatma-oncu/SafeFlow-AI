using MediatR;
using SafeFlow.Application.RiskAssessments.DTOs;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.RiskAssessments.Commands.RejectRiskAssessment;

/// <summary>
/// Command to reject a Risk Assessment in review, returning it to Draft state.
/// </summary>
public sealed record RejectRiskAssessmentCommand(
    Guid Id,
    Guid ReviewerEmployeeId,
    string Comment) : IRequest<Result<RiskAssessmentDto>>;
