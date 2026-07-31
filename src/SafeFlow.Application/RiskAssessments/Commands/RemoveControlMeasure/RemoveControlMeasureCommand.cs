using MediatR;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.RiskAssessments.Commands.RemoveControlMeasure;

/// <summary>
/// Command to remove a control measure from a hazard in a Risk Assessment.
/// </summary>
public sealed record RemoveControlMeasureCommand(
    Guid RiskAssessmentId,
    Guid HazardId,
    Guid ControlMeasureId) : IRequest<Result>;
