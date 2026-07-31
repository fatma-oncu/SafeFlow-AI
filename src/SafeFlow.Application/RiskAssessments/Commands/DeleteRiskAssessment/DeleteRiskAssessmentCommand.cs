using MediatR;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.RiskAssessments.Commands.DeleteRiskAssessment;

/// <summary>
/// Command to soft-delete a Risk Assessment.
/// </summary>
public sealed record DeleteRiskAssessmentCommand(
    Guid Id,
    string? DeletedBy = null) : IRequest<Result>;
