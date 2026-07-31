using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.RiskAssessments.Specifications;
using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.RiskAssessments.Commands.DeleteRiskAssessment;

public sealed class DeleteRiskAssessmentCommandHandler(
    IRepository<RiskAssessment> assessmentRepository,
    IUnitOfWork unitOfWork,
    ILogger<DeleteRiskAssessmentCommandHandler> logger)
    : IRequestHandler<DeleteRiskAssessmentCommand, Result>
{
    public async Task<Result> Handle(
        DeleteRiskAssessmentCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Soft deleting RiskAssessment {AssessmentId}", command.Id);

        var spec = new RiskAssessmentByIdSpecification(command.Id);
        var assessment = await assessmentRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (assessment is null)
        {
            return Result.Failure(RiskAssessmentErrors.NotFound);
        }

        assessment.SoftDelete(command.DeletedBy);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully soft-deleted RiskAssessment {AssessmentId}", assessment.Id);

        return Result.Success();
    }
}
