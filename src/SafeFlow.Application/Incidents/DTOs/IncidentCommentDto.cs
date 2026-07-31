using SafeFlow.Domain.Incidents.Entities;

namespace SafeFlow.Application.Incidents.DTOs;

/// <summary>Data Transfer Object for incident comments.</summary>
public sealed class IncidentCommentDto
{
    public Guid Id { get; init; }
    public Guid IncidentId { get; init; }
    public Guid AuthorEmployeeId { get; init; }
    public string Content { get; init; } = default!;
    public DateTime CreatedAt { get; init; }

    public static IncidentCommentDto FromEntity(IncidentComment entity) => new()
    {
        Id = entity.Id,
        IncidentId = entity.IncidentId,
        AuthorEmployeeId = entity.AuthorEmployeeId,
        Content = entity.Content,
        CreatedAt = entity.CreatedAt
    };
}
