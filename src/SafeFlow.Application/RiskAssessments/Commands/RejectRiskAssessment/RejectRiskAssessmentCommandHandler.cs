using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.RiskAssessments.DTOs;
using SafeFlow.Application.RiskAssessments.Specifications;
using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.RiskAssessments.Commands.RejectRiskAssessment;

public sealed class RejectRiskAssessmentCommandHandler(
    IRepository<RiskAssessment> assessmentRepository,
    IUnitOfWork unitOfWork,
    ILogger<RejectRiskAssessmentCommandHandler> logger)
    : IRequestHandler<RejectRiskAssessmentCommand, Result<RiskAssessmentDto>>
{
    public async Task<Result<RiskAssessmentDto>> Handle(
        RejectRiskAssessmentCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Rejecting RiskAssessment {AssessmentId} by reviewer {ReviewerId}",
            command.Id, command.ReviewerEmployeeId);

        var spec = new RiskAssessmentByIdSpecification(command.Id);
        var assessment = await assessmentRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (assessment is null)
        {
            return Result.Failure<RiskAssessmentDto>(RiskAssessmentErrors.NotFound);
        }

        assessment.Reject(command.ReviewerEmployeeId, command.Comment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully rejected RiskAssessment {AssessmentId}", assessment.Id);

        return Result.Success(RiskAssessmentDto.FromAggregate(assessment));
    }
}
