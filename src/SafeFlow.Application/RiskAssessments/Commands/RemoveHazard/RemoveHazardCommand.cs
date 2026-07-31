using MediatR;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.RiskAssessments.Commands.RemoveHazard;

/// <summary>
/// Command to remove a hazard from a Risk Assessment.
/// </summary>
public sealed record RemoveHazardCommand(
    Guid RiskAssessmentId,
    Guid HazardId) : IRequest<Result>;
