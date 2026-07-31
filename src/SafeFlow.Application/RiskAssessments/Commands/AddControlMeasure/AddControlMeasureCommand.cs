using MediatR;
using SafeFlow.Application.RiskAssessments.DTOs;
using SafeFlow.Domain.RiskAssessments.Enums;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.RiskAssessments.Commands.AddControlMeasure;

/// <summary>
/// Command to add a control measure to a hazard in a Risk Assessment.
/// </summary>
public sealed record AddControlMeasureCommand(
    Guid RiskAssessmentId,
    Guid HazardId,
    string Description,
    ControlMeasureType Type,
    bool IsImplemented) : IRequest<Result<RiskControlMeasureDto>>;
