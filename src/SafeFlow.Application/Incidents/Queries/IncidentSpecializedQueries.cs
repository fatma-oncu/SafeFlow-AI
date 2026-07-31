using MediatR;
using SafeFlow.Application.Incidents.DTOs;
using SafeFlow.Application.Incidents.Specifications;
using SafeFlow.Domain.Incidents.Aggregates;
using SafeFlow.Domain.Incidents.Enums;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Incidents.Queries;

// ── Get Incidents By Status ──────────────────────────────────────────────────
public sealed record GetIncidentsByStatusQuery(IncidentStatus Status) : IRequest<Result<IReadOnlyList<IncidentSearchResultDto>>>;

public sealed class GetIncidentsByStatusQueryHandler(
    IReadRepository<Incident> incidentRepository)
    : IRequestHandler<GetIncidentsByStatusQuery, Result<IReadOnlyList<IncidentSearchResultDto>>>
{
    public async Task<Result<IReadOnlyList<IncidentSearchResultDto>>> Handle(
        GetIncidentsByStatusQuery query,
        CancellationToken cancellationToken)
    {
        var spec = new IncidentByStatusSpecification(query.Status);
        var items = await incidentRepository.ListAsync(spec, cancellationToken);
        var dtos = items.Select(IncidentSearchResultDto.FromAggregate).ToList();
        return Result.Success<IReadOnlyList<IncidentSearchResultDto>>(dtos);
    }
}

// ── Get Incidents By Employee ────────────────────────────────────────────────
public sealed record GetIncidentsByEmployeeQuery(Guid EmployeeId) : IRequest<Result<IReadOnlyList<IncidentSearchResultDto>>>;

public sealed class GetIncidentsByEmployeeQueryHandler(
    IReadRepository<Incident> incidentRepository)
    : IRequestHandler<GetIncidentsByEmployeeQuery, Result<IReadOnlyList<IncidentSearchResultDto>>>
{
    public async Task<Result<IReadOnlyList<IncidentSearchResultDto>>> Handle(
        GetIncidentsByEmployeeQuery query,
        CancellationToken cancellationToken)
    {
        var spec = new IncidentByEmployeeSpecification(query.EmployeeId);
        var items = await incidentRepository.ListAsync(spec, cancellationToken);
        var dtos = items.Select(IncidentSearchResultDto.FromAggregate).ToList();
        return Result.Success<IReadOnlyList<IncidentSearchResultDto>>(dtos);
    }
}

// ── Get Open Incidents ───────────────────────────────────────────────────────
public sealed record GetOpenIncidentsQuery() : IRequest<Result<IReadOnlyList<IncidentSearchResultDto>>>;

public sealed class GetOpenIncidentsQueryHandler(
    IReadRepository<Incident> incidentRepository)
    : IRequestHandler<GetOpenIncidentsQuery, Result<IReadOnlyList<IncidentSearchResultDto>>>
{
    public async Task<Result<IReadOnlyList<IncidentSearchResultDto>>> Handle(
        GetOpenIncidentsQuery query,
        CancellationToken cancellationToken)
    {
        var spec = new OpenIncidentsSpecification();
        var items = await incidentRepository.ListAsync(spec, cancellationToken);
        var dtos = items.Select(IncidentSearchResultDto.FromAggregate).ToList();
        return Result.Success<IReadOnlyList<IncidentSearchResultDto>>(dtos);
    }
}
