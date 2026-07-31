using MediatR;
using SafeFlow.Application.RiskAssessments.DTOs;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.RiskAssessments.Commands.SubmitForReview;

/// <summary>
/// Command to submit a Draft Risk Assessment for review.
/// </summary>
public sealed record SubmitForReviewCommand(
    Guid Id,
    Guid SubmittedByEmployeeId) : IRequest<Result<RiskAssessmentDto>>;
