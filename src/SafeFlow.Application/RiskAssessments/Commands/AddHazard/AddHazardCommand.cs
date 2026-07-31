using MediatR;
using SafeFlow.Application.RiskAssessments.DTOs;
using SafeFlow.Domain.RiskAssessments.Enums;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.RiskAssessments.Commands.AddHazard;

/// <summary>
/// Command to add a hazard to a Risk Assessment.
/// </summary>
public sealed record AddHazardCommand(
    Guid RiskAssessmentId,
    string Description,
    Likelihood InitialLikelihood,
    Severity InitialSeverity,
    Likelihood ResidualLikelihood,
    Severity ResidualSeverity) : IRequest<Result<RiskHazardDto>>;
