using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.Employees.Specifications;
using SafeFlow.Application.Incidents.DTOs;
using SafeFlow.Application.Incidents.Errors;
using SafeFlow.Application.Incidents.Specifications;
using SafeFlow.Domain.Employees.Aggregates;
using SafeFlow.Domain.Incidents.Aggregates;
using SafeFlow.Domain.Incidents.Enums;
using SafeFlow.Domain.Incidents.ValueObjects;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Incidents.Commands;

// ── Assign ──────────────────────────────────────────────────────────────────
public sealed record AssignIncidentCommand(Guid Id, Guid AssignedToEmployeeId) : IRequest<Result<IncidentDto>>;

public sealed class AssignIncidentCommandValidator : AbstractValidator<AssignIncidentCommand>
{
    public AssignIncidentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.AssignedToEmployeeId).NotEmpty();
    }
}

public sealed class AssignIncidentCommandHandler(
    IRepository<Incident> incidentRepository,
    IReadRepository<Employee> employeeRepository,
    IUnitOfWork unitOfWork,
    ILogger<AssignIncidentCommandHandler> logger)
    : IRequestHandler<AssignIncidentCommand, Result<IncidentDto>>
{
    public async Task<Result<IncidentDto>> Handle(AssignIncidentCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Assigning Incident {IncidentId} to Employee {EmployeeId}", command.Id, command.AssignedToEmployeeId);

        var incident = await incidentRepository.FirstOrDefaultAsync(new IncidentByIdSpecification(command.Id), cancellationToken);
        if (incident is null) return Result.Failure<IncidentDto>(IncidentErrors.NotFound);

        var employee = await employeeRepository.FirstOrDefaultAsync(new EmployeeByIdSpecification(command.AssignedToEmployeeId), cancellationToken);
        if (employee is null) return Result.Failure<IncidentDto>(IncidentErrors.EmployeeNotFound);

        incident.Assign(command.AssignedToEmployeeId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(IncidentDto.FromAggregate(incident));
    }
}

// ── Start Investigation ──────────────────────────────────────────────────────
public sealed record StartInvestigationCommand(Guid Id, Guid InvestigatorEmployeeId) : IRequest<Result<IncidentDto>>;

public sealed class StartInvestigationCommandValidator : AbstractValidator<StartInvestigationCommand>
{
    public StartInvestigationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.InvestigatorEmployeeId).NotEmpty();
    }
}

public sealed class StartInvestigationCommandHandler(
    IRepository<Incident> incidentRepository,
    IReadRepository<Employee> employeeRepository,
    IUnitOfWork unitOfWork,
    ILogger<StartInvestigationCommandHandler> logger)
    : IRequestHandler<StartInvestigationCommand, Result<IncidentDto>>
{
    public async Task<Result<IncidentDto>> Handle(StartInvestigationCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting investigation on Incident {IncidentId}", command.Id);

        var incident = await incidentRepository.FirstOrDefaultAsync(new IncidentByIdSpecification(command.Id), cancellationToken);
        if (incident is null) return Result.Failure<IncidentDto>(IncidentErrors.NotFound);

        var investigator = await employeeRepository.FirstOrDefaultAsync(new EmployeeByIdSpecification(command.InvestigatorEmployeeId), cancellationToken);
        if (investigator is null) return Result.Failure<IncidentDto>(IncidentErrors.EmployeeNotFound);

        incident.StartInvestigation(command.InvestigatorEmployeeId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(IncidentDto.FromAggregate(incident));
    }
}

// ── Add Comment ─────────────────────────────────────────────────────────────
public sealed record AddCommentCommand(Guid Id, Guid AuthorEmployeeId, string Content) : IRequest<Result<IncidentCommentDto>>;

public sealed class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    public AddCommentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.AuthorEmployeeId).NotEmpty();
        RuleFor(x => x.Content).NotEmpty().MaximumLength(2000);
    }
}

public sealed class AddCommentCommandHandler(
    IRepository<Incident> incidentRepository,
    IReadRepository<Employee> employeeRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddCommentCommand, Result<IncidentCommentDto>>
{
    public async Task<Result<IncidentCommentDto>> Handle(AddCommentCommand command, CancellationToken cancellationToken)
    {
        var incident = await incidentRepository.FirstOrDefaultAsync(new IncidentByIdSpecification(command.Id), cancellationToken);
        if (incident is null) return Result.Failure<IncidentCommentDto>(IncidentErrors.NotFound);

        var author = await employeeRepository.FirstOrDefaultAsync(new EmployeeByIdSpecification(command.AuthorEmployeeId), cancellationToken);
        if (author is null) return Result.Failure<IncidentCommentDto>(IncidentErrors.EmployeeNotFound);

        var comment = incident.AddComment(command.AuthorEmployeeId, command.Content);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(IncidentCommentDto.FromEntity(comment));
    }
}

// ── Add Attachment ──────────────────────────────────────────────────────────
public sealed record AddAttachmentCommand(
    Guid Id,
    string FileName,
    string FileUrl,
    string ContentType,
    long SizeBytes,
    Guid UploadedByEmployeeId) : IRequest<Result<IncidentAttachmentDto>>;

public sealed class AddAttachmentCommandValidator : AbstractValidator<AddAttachmentCommand>
{
    public AddAttachmentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty();
        RuleFor(x => x.FileUrl).NotEmpty();
        RuleFor(x => x.UploadedByEmployeeId).NotEmpty();
    }
}

public sealed class AddAttachmentCommandHandler(
    IRepository<Incident> incidentRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddAttachmentCommand, Result<IncidentAttachmentDto>>
{
    public async Task<Result<IncidentAttachmentDto>> Handle(AddAttachmentCommand command, CancellationToken cancellationToken)
    {
        var incident = await incidentRepository.FirstOrDefaultAsync(new IncidentByIdSpecification(command.Id), cancellationToken);
        if (incident is null) return Result.Failure<IncidentAttachmentDto>(IncidentErrors.NotFound);

        var attachment = incident.AddAttachment(command.FileName, command.FileUrl, command.ContentType, command.SizeBytes, command.UploadedByEmployeeId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(IncidentAttachmentDto.FromEntity(attachment));
    }
}

// ── Add Corrective Action ────────────────────────────────────────────────────
public sealed record AddCorrectiveActionCommand(
    Guid Id,
    string Description,
    Guid AssignedToEmployeeId,
    DateTime DueDate) : IRequest<Result<CorrectiveActionDto>>;

public sealed class AddCorrectiveActionCommandValidator : AbstractValidator<AddCorrectiveActionCommand>
{
    public AddCorrectiveActionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(CorrectiveActionDescription.MaxLength);
        RuleFor(x => x.AssignedToEmployeeId).NotEmpty();
    }
}

public sealed class AddCorrectiveActionCommandHandler(
    IRepository<Incident> incidentRepository,
    IReadRepository<Employee> employeeRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddCorrectiveActionCommand, Result<CorrectiveActionDto>>
{
    public async Task<Result<CorrectiveActionDto>> Handle(AddCorrectiveActionCommand command, CancellationToken cancellationToken)
    {
        var incident = await incidentRepository.FirstOrDefaultAsync(new IncidentByIdSpecification(command.Id), cancellationToken);
        if (incident is null) return Result.Failure<CorrectiveActionDto>(IncidentErrors.NotFound);

        var assigned = await employeeRepository.FirstOrDefaultAsync(new EmployeeByIdSpecification(command.AssignedToEmployeeId), cancellationToken);
        if (assigned is null) return Result.Failure<CorrectiveActionDto>(IncidentErrors.EmployeeNotFound);

        var desc = CorrectiveActionDescription.Create(command.Description);
        var action = incident.AddCorrectiveAction(desc, command.AssignedToEmployeeId, command.DueDate);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(CorrectiveActionDto.FromEntity(action));
    }
}

// ── Complete Corrective Action ───────────────────────────────────────────────
public sealed record CompleteCorrectiveActionCommand(
    Guid Id,
    Guid ActionId,
    Guid CompletedByEmployeeId) : IRequest<Result<IncidentDto>>;

public sealed class CompleteCorrectiveActionCommandValidator : AbstractValidator<CompleteCorrectiveActionCommand>
{
    public CompleteCorrectiveActionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ActionId).NotEmpty();
        RuleFor(x => x.CompletedByEmployeeId).NotEmpty();
    }
}

public sealed class CompleteCorrectiveActionCommandHandler(
    IRepository<Incident> incidentRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CompleteCorrectiveActionCommand, Result<IncidentDto>>
{
    public async Task<Result<IncidentDto>> Handle(CompleteCorrectiveActionCommand command, CancellationToken cancellationToken)
    {
        var incident = await incidentRepository.FirstOrDefaultAsync(new IncidentByIdSpecification(command.Id), cancellationToken);
        if (incident is null) return Result.Failure<IncidentDto>(IncidentErrors.NotFound);

        incident.CompleteCorrectiveAction(command.ActionId, command.CompletedByEmployeeId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(IncidentDto.FromAggregate(incident));
    }
}

// ── Resolve Incident ────────────────────────────────────────────────────────
public sealed record ResolveIncidentCommand(
    Guid Id,
    InvestigationResult InvestigationResult,
    string ResolutionSummary) : IRequest<Result<IncidentDto>>;

public sealed class ResolveIncidentCommandValidator : AbstractValidator<ResolveIncidentCommand>
{
    public ResolveIncidentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.InvestigationResult).IsInEnum();
        RuleFor(x => x.ResolutionSummary).NotEmpty().MaximumLength(2000);
    }
}

public sealed class ResolveIncidentCommandHandler(
    IRepository<Incident> incidentRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ResolveIncidentCommand, Result<IncidentDto>>
{
    public async Task<Result<IncidentDto>> Handle(ResolveIncidentCommand command, CancellationToken cancellationToken)
    {
        var incident = await incidentRepository.FirstOrDefaultAsync(new IncidentByIdSpecification(command.Id), cancellationToken);
        if (incident is null) return Result.Failure<IncidentDto>(IncidentErrors.NotFound);

        incident.Resolve(command.InvestigationResult, command.ResolutionSummary);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(IncidentDto.FromAggregate(incident));
    }
}

// ── Close Incident ───────────────────────────────────────────────────────────
public sealed record CloseIncidentCommand(
    Guid Id,
    Guid ClosedByEmployeeId,
    string? ClosureNotes) : IRequest<Result<IncidentDto>>;

public sealed class CloseIncidentCommandValidator : AbstractValidator<CloseIncidentCommand>
{
    public CloseIncidentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ClosedByEmployeeId).NotEmpty();
    }
}

public sealed class CloseIncidentCommandHandler(
    IRepository<Incident> incidentRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CloseIncidentCommand, Result<IncidentDto>>
{
    public async Task<Result<IncidentDto>> Handle(CloseIncidentCommand command, CancellationToken cancellationToken)
    {
        var incident = await incidentRepository.FirstOrDefaultAsync(new IncidentByIdSpecification(command.Id), cancellationToken);
        if (incident is null) return Result.Failure<IncidentDto>(IncidentErrors.NotFound);

        incident.Close(command.ClosedByEmployeeId, command.ClosureNotes);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(IncidentDto.FromAggregate(incident));
    }
}

// ── Cancel Incident ──────────────────────────────────────────────────────────
public sealed record CancelIncidentCommand(Guid Id, string Reason) : IRequest<Result<IncidentDto>>;

public sealed class CancelIncidentCommandValidator : AbstractValidator<CancelIncidentCommand>
{
    public CancelIncidentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}

public sealed class CancelIncidentCommandHandler(
    IRepository<Incident> incidentRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CancelIncidentCommand, Result<IncidentDto>>
{
    public async Task<Result<IncidentDto>> Handle(CancelIncidentCommand command, CancellationToken cancellationToken)
    {
        var incident = await incidentRepository.FirstOrDefaultAsync(new IncidentByIdSpecification(command.Id), cancellationToken);
        if (incident is null) return Result.Failure<IncidentDto>(IncidentErrors.NotFound);

        incident.Cancel(command.Reason);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(IncidentDto.FromAggregate(incident));
    }
}

// ── Reopen Incident ──────────────────────────────────────────────────────────
public sealed record ReopenIncidentCommand(Guid Id, string Reason) : IRequest<Result<IncidentDto>>;

public sealed class ReopenIncidentCommandValidator : AbstractValidator<ReopenIncidentCommand>
{
    public ReopenIncidentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}

public sealed class ReopenIncidentCommandHandler(
    IRepository<Incident> incidentRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ReopenIncidentCommand, Result<IncidentDto>>
{
    public async Task<Result<IncidentDto>> Handle(ReopenIncidentCommand command, CancellationToken cancellationToken)
    {
        var incident = await incidentRepository.FirstOrDefaultAsync(new IncidentByIdSpecification(command.Id), cancellationToken);
        if (incident is null) return Result.Failure<IncidentDto>(IncidentErrors.NotFound);

        incident.Reopen(command.Reason);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(IncidentDto.FromAggregate(incident));
    }
}

// ── Delete Incident ──────────────────────────────────────────────────────────
public sealed record DeleteIncidentCommand(Guid Id) : IRequest<Result>;

public sealed class DeleteIncidentCommandHandler(
    IRepository<Incident> incidentRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteIncidentCommand, Result>
{
    public async Task<Result> Handle(DeleteIncidentCommand command, CancellationToken cancellationToken)
    {
        var incident = await incidentRepository.FirstOrDefaultAsync(new IncidentByIdSpecification(command.Id), cancellationToken);
        if (incident is null) return Result.Failure(IncidentErrors.NotFound);

        incident.SoftDelete("System");
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
