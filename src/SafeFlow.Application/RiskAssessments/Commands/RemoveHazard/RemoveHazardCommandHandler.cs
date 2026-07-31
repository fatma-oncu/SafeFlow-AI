using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.RiskAssessments.Specifications;
using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.RiskAssessments.Commands.RemoveHazard;

public sealed class RemoveHazardCommandHandler(
    IRepository<RiskAssessment> assessmentRepository,
    IUnitOfWork unitOfWork,
    ILogger<RemoveHazardCommandHandler> logger)
    : IRequestHandler<RemoveHazardCommand, Result>
{
    public async Task<Result> Handle(
        RemoveHazardCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Removing hazard {HazardId} from RiskAssessment {AssessmentId}",
            command.HazardId, command.RiskAssessmentId);

        var spec = new RiskAssessmentByIdSpecification(command.RiskAssessmentId);
        var assessment = await assessmentRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (assessment is null)
        {
            return Result.Failure(RiskAssessmentErrors.NotFound);
        }

        assessment.RemoveHazard(command.HazardId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully removed hazard {HazardId} from RiskAssessment {AssessmentId}",
            command.HazardId, assessment.Id);

        return Result.Success();
    }
}
