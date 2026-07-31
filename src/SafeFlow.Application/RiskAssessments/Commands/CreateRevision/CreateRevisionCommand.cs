using MediatR;
using SafeFlow.Application.RiskAssessments.DTOs;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.RiskAssessments.Commands.CreateRevision;

/// <summary>
/// Command to create a new revision of an existing Risk Assessment.
/// </summary>
public sealed record CreateRevisionCommand(
    Guid CurrentAssessmentId,
    Guid CreatedByEmployeeId) : IRequest<Result<RiskAssessmentDto>>;
