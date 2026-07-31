using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.RiskAssessments.DTOs;
using SafeFlow.Application.RiskAssessments.Specifications;
using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.Domain.RiskAssessments.ValueObjects;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.RiskAssessments.Commands.AddHazard;

public sealed class AddHazardCommandHandler(
    IRepository<RiskAssessment> assessmentRepository,
    IUnitOfWork unitOfWork,
    ILogger<AddHazardCommandHandler> logger)
    : IRequestHandler<AddHazardCommand, Result<RiskHazardDto>>
{
    public async Task<Result<RiskHazardDto>> Handle(
        AddHazardCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Adding hazard to RiskAssessment {AssessmentId}", command.RiskAssessmentId);

        var spec = new RiskAssessmentByIdSpecification(command.RiskAssessmentId);
        var assessment = await assessmentRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (assessment is null)
        {
            return Result.Failure<RiskHazardDto>(RiskAssessmentErrors.NotFound);
        }

        var description = HazardDescription.Create(command.Description);
        var initialScore = RiskMatrixScore.Calculate(command.InitialLikelihood, command.InitialSeverity);
        var residualScore = RiskMatrixScore.Calculate(command.ResidualLikelihood, command.ResidualSeverity);

        var hazard = assessment.AddHazard(description, initialScore, residualScore);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully added hazard {HazardId} to RiskAssessment {AssessmentId}",
            hazard.Id, assessment.Id);

        return Result.Success(RiskHazardDto.FromEntity(hazard));
    }
}
