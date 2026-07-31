using SafeFlow.Domain.Incidents.Entities;

namespace SafeFlow.Application.Incidents.DTOs;

/// <summary>Data Transfer Object for incident attachments.</summary>
public sealed class IncidentAttachmentDto
{
    public Guid Id { get; init; }
    public Guid IncidentId { get; init; }
    public string FileName { get; init; } = default!;
    public string FileUrl { get; init; } = default!;
    public string ContentType { get; init; } = default!;
    public long SizeBytes { get; init; }
    public Guid UploadedByEmployeeId { get; init; }
    public DateTime CreatedAt { get; init; }

    public static IncidentAttachmentDto FromEntity(IncidentAttachment entity) => new()
    {
        Id = entity.Id,
        IncidentId = entity.IncidentId,
        FileName = entity.FileName,
        FileUrl = entity.FileUrl,
        ContentType = entity.ContentType,
        SizeBytes = entity.SizeBytes,
        UploadedByEmployeeId = entity.UploadedByEmployeeId,
        CreatedAt = entity.CreatedAt
    };
}
