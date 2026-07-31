using MediatR;
using SafeFlow.Application.RiskAssessments.DTOs;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.RiskAssessments.Commands.UpdateRiskAssessment;

/// <summary>
/// Command to update header details of a Risk Assessment with optimistic concurrency control.
/// </summary>
public sealed record UpdateRiskAssessmentCommand(
    Guid Id,
    string Title,
    string Description,
    Guid DepartmentId,
    Guid ResponsibleEmployeeId,
    DateTime? NextReviewDate,
    byte[] RowVersion) : IRequest<Result<RiskAssessmentDto>>;
