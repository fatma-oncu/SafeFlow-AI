using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.Incidents.DTOs;
using SafeFlow.Application.Incidents.Errors;
using SafeFlow.Application.Incidents.Specifications;
using SafeFlow.Domain.Incidents.Aggregates;
using SafeFlow.Domain.Incidents.Enums;
using SafeFlow.Domain.Incidents.ValueObjects;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Incidents.Commands.UpdateIncident;

public sealed record UpdateIncidentCommand(
    Guid Id,
    string Title,
    string Description,
    string Location,
    IncidentSeverity Severity,
    IncidentCategory Category,
    DateTime OccurredAt,
    Guid DepartmentId,
    Guid? RiskAssessmentId,
    Guid? AffectedEmployeeId,
    byte[] RowVersion) : IRequest<Result<IncidentDto>>;

public sealed class UpdateIncidentCommandValidator : AbstractValidator<UpdateIncidentCommand>
{
    public UpdateIncidentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(IncidentTitle.MaxLength);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(IncidentDescription.MaxLength);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(IncidentLocation.MaxLength);
        RuleFor(x => x.DepartmentId).NotEmpty();
    }
}

public sealed class UpdateIncidentCommandHandler(
    IRepository<Incident> incidentRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateIncidentCommandHandler> logger)
    : IRequestHandler<UpdateIncidentCommand, Result<IncidentDto>>
{
    public async Task<Result<IncidentDto>> Handle(
        UpdateIncidentCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating Incident {IncidentId}", command.Id);

        var spec = new IncidentByIdSpecification(command.Id);
        var incident = await incidentRepository.FirstOrDefaultAsync(spec, cancellationToken);
        if (incident is null)
        {
            return Result.Failure<IncidentDto>(IncidentErrors.NotFound);
        }

        // Concurrency check
        if (command.RowVersion is not null && command.RowVersion.Length > 0 &&
            incident.RowVersion is not null && incident.RowVersion.Length > 0 &&
            !command.RowVersion.SequenceEqual(incident.RowVersion))
        {
            return Result.Failure<IncidentDto>(IncidentErrors.ConcurrencyConflict);
        }

        var title = IncidentTitle.Create(command.Title);
        var description = IncidentDescription.Create(command.Description);
        var location = IncidentLocation.Create(command.Location);

        incident.UpdateDetails(
            title,
            description,
            location,
            command.Severity,
            command.Category,
            command.OccurredAt,
            command.DepartmentId,
            command.RiskAssessmentId,
            command.AffectedEmployeeId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully updated Incident {IncidentId}", incident.Id);
        return Result.Success(IncidentDto.FromAggregate(incident));
    }
}
