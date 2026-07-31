using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.RiskAssessments.DTOs;
using SafeFlow.Application.RiskAssessments.Specifications;
using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.RiskAssessments.Commands.ApproveRiskAssessment;

public sealed class ApproveRiskAssessmentCommandHandler(
    IRepository<RiskAssessment> assessmentRepository,
    IUnitOfWork unitOfWork,
    ILogger<ApproveRiskAssessmentCommandHandler> logger)
    : IRequestHandler<ApproveRiskAssessmentCommand, Result<RiskAssessmentDto>>
{
    public async Task<Result<RiskAssessmentDto>> Handle(
        ApproveRiskAssessmentCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Approving RiskAssessment {AssessmentId} by employee {ApproverId}",
            command.Id, command.ApproverEmployeeId);

        var spec = new RiskAssessmentByIdSpecification(command.Id);
        var assessment = await assessmentRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (assessment is null)
        {
            return Result.Failure<RiskAssessmentDto>(RiskAssessmentErrors.NotFound);
        }

        assessment.Approve(command.ApproverEmployeeId, command.Comment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully approved RiskAssessment {AssessmentId}", assessment.Id);

        return Result.Success(RiskAssessmentDto.FromAggregate(assessment));
    }
}
