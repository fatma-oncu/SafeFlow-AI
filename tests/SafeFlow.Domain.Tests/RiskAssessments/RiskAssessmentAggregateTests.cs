using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.Domain.RiskAssessments.Enums;
using SafeFlow.Domain.RiskAssessments.Events;
using SafeFlow.Domain.RiskAssessments.ValueObjects;
using SafeFlow.SharedKernel.Exceptions;
using Xunit;

namespace SafeFlow.Domain.Tests.RiskAssessments;

public class RiskAssessmentAggregateTests
{
    private static (RiskAssessment Assessment, Guid EmployeeId) CreateTestAssessment()
    {
        var num = RiskAssessmentNumber.Create("RA-2026-000001");
        var creatorId = Guid.NewGuid();
        var responsibleId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var deptId = Guid.NewGuid();

        var assessment = RiskAssessment.Create(
            num,
            "Chemical Handling Safety",
            "Assessment for chemical storage area",
            deptId,
            creatorId,
            responsibleId,
            tenantId);

        return (assessment, creatorId);
    }

    [Fact]
    public void Create_ShouldInstantiateInDraftStatusWithDomainEvent()
    {
        var (assessment, creatorId) = CreateTestAssessment();

        Assert.NotNull(assessment);
        Assert.Equal(AssessmentStatus.Draft, assessment.Status);
        Assert.Equal(1, assessment.RevisionNumber);
        Assert.Equal(RiskLevel.Low, assessment.OverallRiskLevel);

        var @event = Assert.Single(assessment.DomainEvents);
        Assert.IsType<RiskAssessmentCreatedDomainEvent>(@event);
    }

    [Fact]
    public void SubmitForReview_WithoutHazards_ShouldThrowValidationException()
    {
        var (assessment, creatorId) = CreateTestAssessment();

        Assert.Throws<ValidationException>(() => assessment.SubmitForReview(creatorId));
    }

    [Fact]
    public void FullApprovalLifecycle_ShouldTransitionStatesAndRecordApprovals()
    {
        var (assessment, creatorId) = CreateTestAssessment();
        var reviewerId = Guid.NewGuid();

        // 1. Add hazard
        var initialScore = RiskMatrixScore.Calculate(Likelihood.Likely, Severity.Critical); // 20 = Critical
        var residualScore = RiskMatrixScore.Calculate(Likelihood.Unlikely, Severity.Minor); // 4 = Low

        assessment.AddHazard(HazardDescription.Create("Acid Spill"), initialScore, residualScore);
        Assert.Equal(RiskLevel.Low, assessment.OverallRiskLevel);

        // 2. Submit for review
        assessment.SubmitForReview(creatorId);
        Assert.Equal(AssessmentStatus.InReview, assessment.Status);

        // 3. Approve
        assessment.Approve(reviewerId, "Approved by Safety Officer");
        Assert.Equal(AssessmentStatus.Approved, assessment.Status);
        Assert.Equal(reviewerId, assessment.ApprovedByEmployeeId);

        // 4. Verify Approval history
        Assert.Equal(2, assessment.Approvals.Count);
        Assert.Equal(ApprovalDecision.Submitted, assessment.Approvals.First().Decision);
        Assert.Equal(ApprovalDecision.Approved, assessment.Approvals.Last().Decision);
    }

    [Fact]
    public void RejectionLifecycle_ShouldTransitionToRejectedStatus()
    {
        var (assessment, creatorId) = CreateTestAssessment();
        var reviewerId = Guid.NewGuid();

        var initialScore = RiskMatrixScore.Calculate(Likelihood.Possible, Severity.Moderate);
        var residualScore = RiskMatrixScore.Calculate(Likelihood.Unlikely, Severity.Minor);

        assessment.AddHazard(HazardDescription.Create("Noise Exposure"), initialScore, residualScore);
        assessment.SubmitForReview(creatorId);

        // Reject
        assessment.Reject(reviewerId, "Missing ear protection control");
        Assert.Equal(AssessmentStatus.Rejected, assessment.Status);

        // Can submit again after corrections
        assessment.SubmitForReview(creatorId);
        Assert.Equal(AssessmentStatus.InReview, assessment.Status);
    }

    [Fact]
    public void CreateRevision_ShouldIncrementRevisionNumberAndCopyHazards()
    {
        var (assessment, creatorId) = CreateTestAssessment();
        var initialScore = RiskMatrixScore.Calculate(Likelihood.Possible, Severity.Moderate);
        var residualScore = RiskMatrixScore.Calculate(Likelihood.Unlikely, Severity.Minor);

        assessment.AddHazard(HazardDescription.Create("Fire Hazard"), initialScore, residualScore);

        var newNum = RiskAssessmentNumber.Create("RA-2026-000002");
        var revision = assessment.CreateRevision(newNum, creatorId);

        Assert.Equal(2, revision.RevisionNumber);
        Assert.Equal(assessment.Id, revision.PreviousAssessmentId);
        Assert.Single(revision.Hazards);
    }

    [Fact]
    public void Archive_ShouldPreventFurtherModifications()
    {
        var (assessment, creatorId) = CreateTestAssessment();

        assessment.Archive();
        Assert.Equal(AssessmentStatus.Archived, assessment.Status);

        var desc = HazardDescription.Create("Slip Hazard");
        var score = RiskMatrixScore.Calculate(Likelihood.Rare, Severity.Negligible);

        Assert.Throws<ValidationException>(() => assessment.AddHazard(desc, score, score));
    }
}
