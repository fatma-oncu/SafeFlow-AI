using MediatR;
using SafeFlow.Application.Incidents.DTOs;
using SafeFlow.Application.Incidents.Specifications;
using SafeFlow.Domain.Incidents.Aggregates;
using SafeFlow.Domain.Incidents.Enums;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Incidents.Queries.SearchIncidents;

public sealed class SearchIncidentsQuery(
    string? searchTerm = null,
    Guid? departmentId = null,
    Guid? reportedByEmployeeId = null,
    Guid? assignedToEmployeeId = null,
    IncidentStatus? status = null,
    IncidentSeverity? severity = null,
    IncidentCategory? category = null,
    DateTime? fromDate = null,
    DateTime? toDate = null,
    int page = 1,
    int pageSize = 20)
    : PagedQuery(page, pageSize), IRequest<Result<PagedResult<IncidentSearchResultDto>>>
{
    public string? SearchTerm { get; } = searchTerm;
    public Guid? DepartmentId { get; } = departmentId;
    public Guid? ReportedByEmployeeId { get; } = reportedByEmployeeId;
    public Guid? AssignedToEmployeeId { get; } = assignedToEmployeeId;
    public IncidentStatus? Status { get; } = status;
    public IncidentSeverity? Severity { get; } = severity;
    public IncidentCategory? Category { get; } = category;
    public DateTime? FromDate { get; } = fromDate;
    public DateTime? ToDate { get; } = toDate;
}

public sealed class SearchIncidentsQueryHandler(
    IReadRepository<Incident> incidentRepository)
    : IRequestHandler<SearchIncidentsQuery, Result<PagedResult<IncidentSearchResultDto>>>
{
    public async Task<Result<PagedResult<IncidentSearchResultDto>>> Handle(
        SearchIncidentsQuery query,
        CancellationToken cancellationToken)
    {
        var searchSpec = new IncidentSearchSpecification(
            query.SearchTerm,
            query.DepartmentId,
            query.ReportedByEmployeeId,
            query.AssignedToEmployeeId,
            query.Status,
            query.Severity,
            query.Category,
            query.FromDate,
            query.ToDate,
            query.Page,
            query.PageSize);

        var countSpec = new IncidentSearchCountSpecification(
            query.SearchTerm,
            query.DepartmentId,
            query.ReportedByEmployeeId,
            query.AssignedToEmployeeId,
            query.Status,
            query.Severity,
            query.Category,
            query.FromDate,
            query.ToDate);

        var items = await incidentRepository.ListAsync(searchSpec, cancellationToken);
        int totalCount = await incidentRepository.CountAsync(countSpec, cancellationToken);

        var dtos = items.Select(IncidentSearchResultDto.FromAggregate).ToList();
        var pagedResult = PagedResult<IncidentSearchResultDto>.Create(dtos, totalCount, query.Page, query.PageSize);

        return Result.Success(pagedResult);
    }
}
