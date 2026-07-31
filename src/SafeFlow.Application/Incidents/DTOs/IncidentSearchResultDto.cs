using SafeFlow.Domain.Incidents.Aggregates;

namespace SafeFlow.Application.Incidents.DTOs;

/// <summary>Summary DTO for incident lists and search results.</summary>
public sealed class IncidentSearchResultDto
{
    public Guid Id { get; init; }
    public string IncidentNumber { get; init; } = default!;
    public string Title { get; init; } = default!;
    public string Location { get; init; } = default!;
    public DateTime OccurredAt { get; init; }
    public string Severity { get; init; } = default!;
    public string Category { get; init; } = default!;
    public string Status { get; init; } = default!;
    public Guid DepartmentId { get; init; }
    public Guid ReportedByEmployeeId { get; init; }
    public Guid? AssignedToEmployeeId { get; init; }
    public DateTime CreatedAt { get; init; }

    public static IncidentSearchResultDto FromAggregate(Incident aggregate) => new()
    {
        Id = aggregate.Id,
        IncidentNumber = aggregate.IncidentNumber.Value,
        Title = aggregate.Title.Value,
        Location = aggregate.Location.Value,
        OccurredAt = aggregate.OccurredAt,
        Severity = aggregate.Severity.ToString(),
        Category = aggregate.Category.ToString(),
        Status = aggregate.Status.ToString(),
        DepartmentId = aggregate.DepartmentId,
        ReportedByEmployeeId = aggregate.ReportedByEmployeeId,
        AssignedToEmployeeId = aggregate.AssignedToEmployeeId,
        CreatedAt = aggregate.CreatedAt
    };
}
