using SafeFlow.SharedKernel.Entities;

namespace SafeFlow.Domain.Incidents.Entities;

/// <summary>
/// Represents an attached photo or document for an Incident.
/// </summary>
public sealed class IncidentAttachment : BaseEntity
{
    private IncidentAttachment() { }

    public Guid IncidentId { get; private set; }
    public string FileName { get; private set; } = default!;
    public string FileUrl { get; private set; } = default!;
    public string ContentType { get; private set; } = default!;
    public long SizeBytes { get; private set; }
    public Guid UploadedByEmployeeId { get; private set; }

    internal static IncidentAttachment Create(
        Guid incidentId,
        string fileName,
        string fileUrl,
        string contentType,
        long sizeBytes,
        Guid uploadedByEmployeeId)
    {
        if (incidentId == Guid.Empty)
            throw new ArgumentException("IncidentId must not be empty.", nameof(incidentId));

        ArgumentException.ThrowIfNullOrWhiteSpace(fileName, nameof(fileName));
        ArgumentException.ThrowIfNullOrWhiteSpace(fileUrl, nameof(fileUrl));

        if (uploadedByEmployeeId == Guid.Empty)
            throw new ArgumentException("UploadedByEmployeeId must not be empty.", nameof(uploadedByEmployeeId));

        return new IncidentAttachment
        {
            Id = Guid.NewGuid(),
            IncidentId = incidentId,
            FileName = fileName.Trim(),
            FileUrl = fileUrl.Trim(),
            ContentType = contentType?.Trim() ?? "application/octet-stream",
            SizeBytes = sizeBytes > 0 ? sizeBytes : 0,
            UploadedByEmployeeId = uploadedByEmployeeId
        };
    }
}
