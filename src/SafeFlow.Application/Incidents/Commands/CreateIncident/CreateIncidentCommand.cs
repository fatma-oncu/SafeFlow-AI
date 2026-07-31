using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.Employees.Specifications;
using SafeFlow.Application.Incidents.DTOs;
using SafeFlow.Application.Incidents.Errors;
using SafeFlow.Application.Incidents.Interfaces;
using SafeFlow.Application.Incidents.Specifications;
using SafeFlow.Domain.Employees.Aggregates;
using SafeFlow.Domain.Incidents.Aggregates;
using SafeFlow.Domain.Incidents.Enums;
using SafeFlow.Domain.Incidents.ValueObjects;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Incidents.Commands.CreateIncident;

public sealed record CreateIncidentCommand(
    string Title,
    string Description,
    string Location,
    IncidentSeverity Severity,
    IncidentCategory Category,
    DateTime OccurredAt,
    Guid DepartmentId,
    Guid ReportedByEmployeeId,
    Guid TenantId,
    Guid? RiskAssessmentId = null,
    Guid? AffectedEmployeeId = null) : IRequest<Result<IncidentDto>>;

public sealed class CreateIncidentCommandValidator : AbstractValidator<CreateIncidentCommand>
{
    public CreateIncidentCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(IncidentTitle.MaxLength);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(IncidentDescription.MaxLength);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(IncidentLocation.MaxLength);
        RuleFor(x => x.Severity).IsInEnum();
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.DepartmentId).NotEmpty();
        RuleFor(x => x.ReportedByEmployeeId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
    }
}

public sealed class CreateIncidentCommandHandler(
    IRepository<Incident> incidentRepository,
    IReadRepository<Employee> employeeRepository,
    IIncidentNumberGenerator numberGenerator,
    IUnitOfWork unitOfWork,
    ILogger<CreateIncidentCommandHandler> logger)
    : IRequestHandler<CreateIncidentCommand, Result<IncidentDto>>
{
    public async Task<Result<IncidentDto>> Handle(
        CreateIncidentCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating Incident for title '{Title}'", command.Title);

        var reporter = await employeeRepository.FirstOrDefaultAsync(
            new EmployeeByIdSpecification(command.ReportedByEmployeeId), cancellationToken);
        if (reporter is null)
        {
            return Result.Failure<IncidentDto>(IncidentErrors.EmployeeNotFound);
        }

        if (command.AffectedEmployeeId.HasValue && command.AffectedEmployeeId.Value != Guid.Empty)
        {
            var affected = await employeeRepository.FirstOrDefaultAsync(
                new EmployeeByIdSpecification(command.AffectedEmployeeId.Value), cancellationToken);
            if (affected is null)
            {
                return Result.Failure<IncidentDto>(IncidentErrors.EmployeeNotFound);
            }
        }

        string generatedNumber = await numberGenerator.GenerateNextNumberAsync(cancellationToken);
        var existingSpec = new IncidentByNumberSpecification(generatedNumber);
        var existing = await incidentRepository.FirstOrDefaultAsync(existingSpec, cancellationToken);
        if (existing is not null)
        {
            return Result.Failure<IncidentDto>(IncidentErrors.NumberAlreadyExists);
        }

        var number = IncidentNumber.Create(generatedNumber);
        var title = IncidentTitle.Create(command.Title);
        var description = IncidentDescription.Create(command.Description);
        var location = IncidentLocation.Create(command.Location);

        var incident = Incident.Create(
            number,
            title,
            description,
            location,
            command.Severity,
            command.Category,
            command.OccurredAt,
            command.DepartmentId,
            command.ReportedByEmployeeId,
            command.TenantId,
            command.RiskAssessmentId,
            command.AffectedEmployeeId);

        await incidentRepository.AddAsync(incident, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully created Incident {IncidentId} ({Number})", incident.Id, incident.IncidentNumber.Value);
        return Result.Success(IncidentDto.FromAggregate(incident));
    }
}
