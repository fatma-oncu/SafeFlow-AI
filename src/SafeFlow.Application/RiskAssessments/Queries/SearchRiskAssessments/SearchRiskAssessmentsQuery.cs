using MediatR;
using SafeFlow.Application.RiskAssessments.DTOs;
using SafeFlow.Domain.RiskAssessments.Enums;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.RiskAssessments.Queries.SearchRiskAssessments;

/// <summary>
/// Query for enterprise multi-criterion search and filtering across Risk Assessments.
/// </summary>
public sealed class SearchRiskAssessmentsQuery(
    string? searchTerm = null,
    Guid? departmentId = null,
    Guid? responsibleEmployeeId = null,
    Guid? createdByEmployeeId = null,
    string? assessmentNumber = null,
    AssessmentStatus? status = null,
    RiskLevel? riskLevel = null,
    DateTime? fromDate = null,
    DateTime? toDate = null,
    int page = 1,
    int pageSize = 20)
    : PagedQuery(page, pageSize), IRequest<Result<PagedResult<RiskAssessmentSearchResultDto>>>
{
    public string? SearchTerm { get; } = searchTerm;
    public Guid? DepartmentId { get; } = departmentId;
    public Guid? ResponsibleEmployeeId { get; } = responsibleEmployeeId;
    public Guid? CreatedByEmployeeId { get; } = createdByEmployeeId;
    public string? AssessmentNumber { get; } = assessmentNumber;
    public AssessmentStatus? Status { get; } = status;
    public RiskLevel? RiskLevel { get; } = riskLevel;
    public DateTime? FromDate { get; } = fromDate;
    public DateTime? ToDate { get; } = toDate;
}
