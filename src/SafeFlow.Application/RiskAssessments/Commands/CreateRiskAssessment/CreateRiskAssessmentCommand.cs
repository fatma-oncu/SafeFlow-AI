using MediatR;
using SafeFlow.Application.RiskAssessments.DTOs;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.RiskAssessments.Commands.CreateRiskAssessment;

/// <summary>
/// Command to create a new Risk Assessment in Draft status.
/// </summary>
public sealed record CreateRiskAssessmentCommand(
    string Title,
    string Description,
    Guid DepartmentId,
    Guid CreatedByEmployeeId,
    Guid ResponsibleEmployeeId,
    Guid TenantId,
    DateTime? NextReviewDate) : IRequest<Result<RiskAssessmentDto>>;
