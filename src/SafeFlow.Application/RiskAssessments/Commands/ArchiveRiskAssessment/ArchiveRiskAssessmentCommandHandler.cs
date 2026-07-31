using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.RiskAssessments.DTOs;
using SafeFlow.Application.RiskAssessments.Specifications;
using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.RiskAssessments.Commands.ArchiveRiskAssessment;

public sealed class ArchiveRiskAssessmentCommandHandler(
    IRepository<RiskAssessment> assessmentRepository,
    IUnitOfWork unitOfWork,
    ILogger<ArchiveRiskAssessmentCommandHandler> logger)
    : IRequestHandler<ArchiveRiskAssessmentCommand, Result<RiskAssessmentDto>>
{
    public async Task<Result<RiskAssessmentDto>> Handle(
        ArchiveRiskAssessmentCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Archiving RiskAssessment {AssessmentId}", command.Id);

        var spec = new RiskAssessmentByIdSpecification(command.Id);
        var assessment = await assessmentRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (assessment is null)
        {
            return Result.Failure<RiskAssessmentDto>(RiskAssessmentErrors.NotFound);
        }

        assessment.Archive();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully archived RiskAssessment {AssessmentId}", assessment.Id);

        return Result.Success(RiskAssessmentDto.FromAggregate(assessment));
    }
}
