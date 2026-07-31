using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.RiskAssessments.DTOs;
using SafeFlow.Application.RiskAssessments.Interfaces;
using SafeFlow.Application.RiskAssessments.Specifications;
using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.Domain.RiskAssessments.ValueObjects;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.RiskAssessments.Commands.CreateRevision;

public sealed class CreateRevisionCommandHandler(
    IRepository<RiskAssessment> assessmentRepository,
    IRiskAssessmentNumberGenerator numberGenerator,
    IUnitOfWork unitOfWork,
    ILogger<CreateRevisionCommandHandler> logger)
    : IRequestHandler<CreateRevisionCommand, Result<RiskAssessmentDto>>
{
    public async Task<Result<RiskAssessmentDto>> Handle(
        CreateRevisionCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating revision for assessment {AssessmentId}", command.CurrentAssessmentId);

        var spec = new RiskAssessmentByIdSpecification(command.CurrentAssessmentId);
        var current = await assessmentRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (current is null)
        {
            return Result.Failure<RiskAssessmentDto>(RiskAssessmentErrors.NotFound);
        }

        string generatedNumber = await numberGenerator.GenerateNextNumberAsync(cancellationToken);
        var number = RiskAssessmentNumber.Create(generatedNumber);

        var revision = current.CreateRevision(number, command.CreatedByEmployeeId);

        await assessmentRepository.AddAsync(revision, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully created revision {RevisionNumber} ({Number}) for assessment {PreviousId}",
            revision.RevisionNumber, revision.AssessmentNumber.Value, current.Id);

        return Result.Success(RiskAssessmentDto.FromAggregate(revision));
    }
}
