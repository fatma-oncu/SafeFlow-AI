using MediatR;
using SafeFlow.Application.RiskAssessments.DTOs;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.RiskAssessments.Commands.ApproveRiskAssessment;

/// <summary>
/// Command to approve a Risk Assessment currently in review.
/// </summary>
public sealed record ApproveRiskAssessmentCommand(
    Guid Id,
    Guid ApproverEmployeeId,
    string? Comment) : IRequest<Result<RiskAssessmentDto>>;
