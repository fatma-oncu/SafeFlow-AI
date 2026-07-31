using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.RiskAssessments.DTOs;
using SafeFlow.Application.RiskAssessments.Specifications;
using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.RiskAssessments.Commands.SubmitForReview;

public sealed class SubmitForReviewCommandHandler(
    IRepository<RiskAssessment> assessmentRepository,
    IUnitOfWork unitOfWork,
    ILogger<SubmitForReviewCommandHandler> logger)
    : IRequestHandler<SubmitForReviewCommand, Result<RiskAssessmentDto>>
{
    public async Task<Result<RiskAssessmentDto>> Handle(
        SubmitForReviewCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Submitting RiskAssessment {AssessmentId} for review", command.Id);

        var spec = new RiskAssessmentByIdSpecification(command.Id);
        var assessment = await assessmentRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (assessment is null)
        {
            return Result.Failure<RiskAssessmentDto>(RiskAssessmentErrors.NotFound);
        }

        assessment.SubmitForReview(command.SubmittedByEmployeeId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully submitted RiskAssessment {AssessmentId} for review", assessment.Id);

        return Result.Success(RiskAssessmentDto.FromAggregate(assessment));
    }
}
