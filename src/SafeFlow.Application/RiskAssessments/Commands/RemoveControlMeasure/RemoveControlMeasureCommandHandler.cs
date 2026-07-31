using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.RiskAssessments.Specifications;
using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.RiskAssessments.Commands.RemoveControlMeasure;

public sealed class RemoveControlMeasureCommandHandler(
    IRepository<RiskAssessment> assessmentRepository,
    IUnitOfWork unitOfWork,
    ILogger<RemoveControlMeasureCommandHandler> logger)
    : IRequestHandler<RemoveControlMeasureCommand, Result>
{
    public async Task<Result> Handle(
        RemoveControlMeasureCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Removing control measure {ControlId} from hazard {HazardId} in assessment {AssessmentId}",
            command.ControlMeasureId, command.HazardId, command.RiskAssessmentId);

        var spec = new RiskAssessmentByIdSpecification(command.RiskAssessmentId);
        var assessment = await assessmentRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (assessment is null)
        {
            return Result.Failure(RiskAssessmentErrors.NotFound);
        }

        assessment.RemoveControlMeasure(command.HazardId, command.ControlMeasureId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully removed control measure {ControlId}", command.ControlMeasureId);

        return Result.Success();
    }
}
