using MediatR;
using SafeFlow.Application.RiskAssessments.DTOs;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.RiskAssessments.Commands.ArchiveRiskAssessment;

/// <summary>
/// Command to archive a Risk Assessment.
/// </summary>
public sealed record ArchiveRiskAssessmentCommand(Guid Id) : IRequest<Result<RiskAssessmentDto>>;
