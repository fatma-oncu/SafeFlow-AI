using MediatR;
using SafeFlow.Application.RiskAssessments.DTOs;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.RiskAssessments.Queries.GetRiskAssessmentById;

/// <summary>
/// Query to retrieve a <see cref="RiskAssessmentDto"/> by ID.
/// </summary>
public sealed record GetRiskAssessmentByIdQuery(Guid Id) : IRequest<Result<RiskAssessmentDto>>;
