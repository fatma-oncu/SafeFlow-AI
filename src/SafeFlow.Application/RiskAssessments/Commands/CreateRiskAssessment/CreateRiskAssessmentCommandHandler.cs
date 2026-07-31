using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.Employees.Specifications;
using SafeFlow.Application.RiskAssessments.DTOs;
using SafeFlow.Application.RiskAssessments.Interfaces;
using SafeFlow.Application.RiskAssessments.Specifications;
using SafeFlow.Domain.Employees.Aggregates;
using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.Domain.RiskAssessments.ValueObjects;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.RiskAssessments.Commands.CreateRiskAssessment;

public sealed class CreateRiskAssessmentCommandHandler(
    IRepository<RiskAssessment> assessmentRepository,
    IReadRepository<Employee> employeeRepository,
    IRiskAssessmentNumberGenerator numberGenerator,
    IUnitOfWork unitOfWork,
    ILogger<CreateRiskAssessmentCommandHandler> logger)
    : IRequestHandler<CreateRiskAssessmentCommand, Result<RiskAssessmentDto>>
{
    public async Task<Result<RiskAssessmentDto>> Handle(
        CreateRiskAssessmentCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating RiskAssessment for title '{Title}'", command.Title);

        // 1. Verify creator employee exists
        var creator = await employeeRepository.FirstOrDefaultAsync(
            new EmployeeByIdSpecification(command.CreatedByEmployeeId), cancellationToken);
        if (creator is null)
        {
            return Result.Failure<RiskAssessmentDto>(RiskAssessmentErrors.EmployeeNotFound);
        }

        // 2. Verify responsible employee exists
        var responsible = await employeeRepository.FirstOrDefaultAsync(
            new EmployeeByIdSpecification(command.ResponsibleEmployeeId), cancellationToken);
        if (responsible is null)
        {
            return Result.Failure<RiskAssessmentDto>(RiskAssessmentErrors.EmployeeNotFound);
        }

        // 3. Generate assessment number
        string generatedNumber = await numberGenerator.GenerateNextNumberAsync(cancellationToken);

        // 4. Check assessment number uniqueness
        var existingSpec = new RiskAssessmentByNumberSpecification(generatedNumber);
        var existing = await assessmentRepository.FirstOrDefaultAsync(existingSpec, cancellationToken);
        if (existing is not null)
        {
            return Result.Failure<RiskAssessmentDto>(RiskAssessmentErrors.NumberAlreadyExists);
        }

        // 5. Instantiate aggregate
        var number = RiskAssessmentNumber.Create(generatedNumber);
        var assessment = RiskAssessment.Create(
            number,
            command.Title,
            command.Description,
            command.DepartmentId,
            command.CreatedByEmployeeId,
            command.ResponsibleEmployeeId,
            command.TenantId,
            command.NextReviewDate);

        await assessmentRepository.AddAsync(assessment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully created RiskAssessment {AssessmentId} ({Number})",
            assessment.Id, assessment.AssessmentNumber.Value);

        return Result.Success(RiskAssessmentDto.FromAggregate(assessment));
    }
}
