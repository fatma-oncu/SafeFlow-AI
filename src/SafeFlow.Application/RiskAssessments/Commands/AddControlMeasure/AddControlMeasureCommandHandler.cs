using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.RiskAssessments.DTOs;
using SafeFlow.Application.RiskAssessments.Specifications;
using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.Domain.RiskAssessments.ValueObjects;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.RiskAssessments.Commands.AddControlMeasure;

public sealed class AddControlMeasureCommandHandler(
    IRepository<RiskAssessment> assessmentRepository,
    IUnitOfWork unitOfWork,
    ILogger<AddControlMeasureCommandHandler> logger)
    : IRequestHandler<AddControlMeasureCommand, Result<RiskControlMeasureDto>>
{
    public async Task<Result<RiskControlMeasureDto>> Handle(
        AddControlMeasureCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Adding control measure to hazard {HazardId} in assessment {AssessmentId}",
            command.HazardId, command.RiskAssessmentId);

        var spec = new RiskAssessmentByIdSpecification(command.RiskAssessmentId);
        var assessment = await assessmentRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (assessment is null)
        {
            return Result.Failure<RiskControlMeasureDto>(RiskAssessmentErrors.NotFound);
        }

        var description = ControlDescription.Create(command.Description);
        var control = assessment.AddControlMeasure(command.HazardId, description, command.Type, command.IsImplemented);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully added control measure {ControlId} to hazard {HazardId}",
            control.Id, command.HazardId);

        return Result.Success(RiskControlMeasureDto.FromEntity(control));
    }
}
