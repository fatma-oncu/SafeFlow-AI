using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.Employees.Specifications;
using SafeFlow.Application.RiskAssessments.DTOs;
using SafeFlow.Application.RiskAssessments.Specifications;
using SafeFlow.Domain.Employees.Aggregates;
using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.RiskAssessments.Commands.UpdateRiskAssessment;

public sealed class UpdateRiskAssessmentCommandHandler(
    IRepository<RiskAssessment> assessmentRepository,
    IReadRepository<Employee> employeeRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateRiskAssessmentCommandHandler> logger)
    : IRequestHandler<UpdateRiskAssessmentCommand, Result<RiskAssessmentDto>>
{
    public async Task<Result<RiskAssessmentDto>> Handle(
        UpdateRiskAssessmentCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating RiskAssessment {AssessmentId}", command.Id);

        var spec = new RiskAssessmentByIdSpecification(command.Id);
        var assessment = await assessmentRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (assessment is null)
        {
            return Result.Failure<RiskAssessmentDto>(RiskAssessmentErrors.NotFound);
        }

        // Concurrency check
        if (command.RowVersion is not null && command.RowVersion.Length > 0 &&
            assessment.RowVersion is not null && assessment.RowVersion.Length > 0 &&
            !command.RowVersion.SequenceEqual(assessment.RowVersion))
        {
            return Result.Failure<RiskAssessmentDto>(Error.Conflict(
                "RiskAssessment.ConcurrencyConflict",
                "The risk assessment has been modified by another user. Please reload and try again."));
        }

        // Verify responsible employee exists
        var responsible = await employeeRepository.FirstOrDefaultAsync(
            new EmployeeByIdSpecification(command.ResponsibleEmployeeId), cancellationToken);
        if (responsible is null)
        {
            return Result.Failure<RiskAssessmentDto>(RiskAssessmentErrors.EmployeeNotFound);
        }

        assessment.UpdateDetails(
            command.Title,
            command.Description,
            command.DepartmentId,
            command.ResponsibleEmployeeId,
            command.NextReviewDate);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully updated RiskAssessment {AssessmentId}", assessment.Id);

        return Result.Success(RiskAssessmentDto.FromAggregate(assessment));
    }
}
