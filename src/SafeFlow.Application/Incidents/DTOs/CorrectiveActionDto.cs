using SafeFlow.Domain.Incidents.Entities;

namespace SafeFlow.Application.Incidents.DTOs;

/// <summary>Data Transfer Object for corrective actions.</summary>
public sealed class CorrectiveActionDto
{
    public Guid Id { get; init; }
    public Guid IncidentId { get; init; }
    public string Description { get; init; } = default!;
    public Guid AssignedToEmployeeId { get; init; }
    public DateTime DueDate { get; init; }
    public DateTime? CompletedAt { get; init; }
    public Guid? CompletedByEmployeeId { get; init; }
    public string Status { get; init; } = default!;
    public DateTime CreatedAt { get; init; }

    public static CorrectiveActionDto FromEntity(CorrectiveAction entity) => new()
    {
        Id = entity.Id,
        IncidentId = entity.IncidentId,
        Description = entity.Description.Value,
        AssignedToEmployeeId = entity.AssignedToEmployeeId,
        DueDate = entity.DueDate,
        CompletedAt = entity.CompletedAt,
        CompletedByEmployeeId = entity.CompletedByEmployeeId,
        Status = entity.Status.ToString(),
        CreatedAt = entity.CreatedAt
    };
}
