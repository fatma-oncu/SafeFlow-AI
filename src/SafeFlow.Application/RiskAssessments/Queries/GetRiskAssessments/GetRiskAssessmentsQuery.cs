using MediatR;
using SafeFlow.Application.RiskAssessments.DTOs;
using SafeFlow.Domain.RiskAssessments.Enums;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.RiskAssessments.Queries.GetRiskAssessments;

/// <summary>
/// Query to return a paged list of Risk Assessments with optional department/status filters.
/// </summary>
public sealed class GetRiskAssessmentsQuery(
    int page = 1,
    int pageSize = 20,
    Guid? departmentId = null,
    AssessmentStatus? status = null)
    : PagedQuery(page, pageSize), IRequest<Result<PagedResult<RiskAssessmentSearchResultDto>>>
{
    public Guid? DepartmentId { get; } = departmentId;
    public AssessmentStatus? Status { get; } = status;
}
