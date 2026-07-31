using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.Domain.RiskAssessments.Enums;
using SafeFlow.SharedKernel.Specifications;

namespace SafeFlow.Application.RiskAssessments.Specifications;

/// <summary>
/// Specification for enterprise multi-criterion search across risk assessments.
/// </summary>
public sealed class RiskAssessmentSearchSpecification : BaseSpecification<RiskAssessment>
{
    public RiskAssessmentSearchSpecification(
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
        : base(r =>
            (string.IsNullOrWhiteSpace(searchTerm) ||
                r.AssessmentNumber.Value.Contains(searchTerm.Trim()) ||
                r.Title.Contains(searchTerm.Trim()) ||
                r.Description.Contains(searchTerm.Trim())) &&
            (string.IsNullOrWhiteSpace(assessmentNumber) || r.AssessmentNumber.Value == assessmentNumber.Trim()) &&
            (!departmentId.HasValue || r.DepartmentId == departmentId.Value) &&
            (!responsibleEmployeeId.HasValue || r.ResponsibleEmployeeId == responsibleEmployeeId.Value) &&
            (!createdByEmployeeId.HasValue || r.CreatedByEmployeeId == createdByEmployeeId.Value) &&
            (!status.HasValue || r.Status == status.Value) &&
            (!riskLevel.HasValue || r.OverallRiskLevel == riskLevel.Value) &&
            (!fromDate.HasValue || r.CreatedAt >= fromDate.Value) &&
            (!toDate.HasValue || r.CreatedAt <= toDate.Value))
    {
        ApplyOrderByDescending(r => r.CreatedAt);
        ApplyPaging((page - 1) * pageSize, pageSize);
        ApplyNoTracking();
    }
}

/// <summary>
/// Specification for counting total search results matching search criteria without paging.
/// </summary>
public sealed class RiskAssessmentSearchCountSpecification : BaseSpecification<RiskAssessment>
{
    public RiskAssessmentSearchCountSpecification(
        string? searchTerm = null,
        Guid? departmentId = null,
        Guid? responsibleEmployeeId = null,
        Guid? createdByEmployeeId = null,
        string? assessmentNumber = null,
        AssessmentStatus? status = null,
        RiskLevel? riskLevel = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
        : base(r =>
            (string.IsNullOrWhiteSpace(searchTerm) ||
                r.AssessmentNumber.Value.Contains(searchTerm.Trim()) ||
                r.Title.Contains(searchTerm.Trim()) ||
                r.Description.Contains(searchTerm.Trim())) &&
            (string.IsNullOrWhiteSpace(assessmentNumber) || r.AssessmentNumber.Value == assessmentNumber.Trim()) &&
            (!departmentId.HasValue || r.DepartmentId == departmentId.Value) &&
            (!responsibleEmployeeId.HasValue || r.ResponsibleEmployeeId == responsibleEmployeeId.Value) &&
            (!createdByEmployeeId.HasValue || r.CreatedByEmployeeId == createdByEmployeeId.Value) &&
            (!status.HasValue || r.Status == status.Value) &&
            (!riskLevel.HasValue || r.OverallRiskLevel == riskLevel.Value) &&
            (!fromDate.HasValue || r.CreatedAt >= fromDate.Value) &&
            (!toDate.HasValue || r.CreatedAt <= toDate.Value))
    {
        ApplyNoTracking();
    }
}
