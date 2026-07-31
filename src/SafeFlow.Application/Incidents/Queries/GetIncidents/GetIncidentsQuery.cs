using MediatR;
using SafeFlow.Application.Incidents.DTOs;
using SafeFlow.Application.Incidents.Specifications;
using SafeFlow.Domain.Incidents.Aggregates;
using SafeFlow.Domain.Incidents.Enums;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Incidents.Queries.GetIncidents;

public sealed class GetIncidentsQuery(
    int page = 1,
    int pageSize = 20,
    Guid? departmentId = null,
    IncidentStatus? status = null)
    : PagedQuery(page, pageSize), IRequest<Result<PagedResult<IncidentSearchResultDto>>>
{
    public Guid? DepartmentId { get; } = departmentId;
    public IncidentStatus? Status { get; } = status;
}

public sealed class GetIncidentsQueryHandler(
    IReadRepository<Incident> incidentRepository)
    : IRequestHandler<GetIncidentsQuery, Result<PagedResult<IncidentSearchResultDto>>>
{
    public async Task<Result<PagedResult<IncidentSearchResultDto>>> Handle(
        GetIncidentsQuery query,
        CancellationToken cancellationToken)
    {
        var pagedSpec = new IncidentPagedSpecification(query.Page, query.PageSize, query.DepartmentId, query.Status);
        var countSpec = new IncidentSearchCountSpecification(departmentId: query.DepartmentId, status: query.Status);

        var items = await incidentRepository.ListAsync(pagedSpec, cancellationToken);
        int totalCount = await incidentRepository.CountAsync(countSpec, cancellationToken);

        var dtos = items.Select(IncidentSearchResultDto.FromAggregate).ToList();
        var pagedResult = PagedResult<IncidentSearchResultDto>.Create(dtos, totalCount, query.Page, query.PageSize);

        return Result.Success(pagedResult);
    }
}
