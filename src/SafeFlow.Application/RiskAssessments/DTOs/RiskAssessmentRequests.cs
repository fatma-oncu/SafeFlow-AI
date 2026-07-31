using SafeFlow.Domain.RiskAssessments.Enums;

namespace SafeFlow.Application.RiskAssessments.DTOs;

/// <summary>
/// HTTP API request DTOs for Risk Assessment endpoints.
/// </summary>
public static class RiskAssessmentRequests
{
    public sealed record CreateRiskAssessmentRequest(
        string Title,
        string Description,
        Guid DepartmentId,
        Guid CreatedByEmployeeId,
        Guid ResponsibleEmployeeId,
        Guid TenantId,
        DateTime? NextReviewDate);

    public sealed record CreateRevisionRequest(
        Guid CreatedByEmployeeId);

    public sealed record UpdateRiskAssessmentRequest(
        string Title,
        string Description,
        Guid DepartmentId,
        Guid ResponsibleEmployeeId,
        DateTime? NextReviewDate,
        byte[] RowVersion);

    public sealed record ApproveRequest(
        Guid ApproverEmployeeId,
        string? Comment);

    public sealed record RejectRequest(
        Guid ReviewerEmployeeId,
        string Comment);

    public sealed record AddHazardRequest(
        string Description,
        Likelihood InitialLikelihood,
        Severity InitialSeverity,
        Likelihood ResidualLikelihood,
        Severity ResidualSeverity);

    public sealed record AddControlMeasureRequest(
        string Description,
        ControlMeasureType Type,
        bool IsImplemented);
}
