using SafeFlow.SharedKernel.Entities;

namespace SafeFlow.Domain.Incidents.Entities;

/// <summary>
/// Represents a comment or update posted on an Incident.
/// </summary>
public sealed class IncidentComment : BaseEntity
{
    private IncidentComment() { }

    public Guid IncidentId { get; private set; }
    public Guid AuthorEmployeeId { get; private set; }
    public string Content { get; private set; } = default!;

    internal static IncidentComment Create(
        Guid incidentId,
        Guid authorEmployeeId,
        string content)
    {
        if (incidentId == Guid.Empty)
            throw new ArgumentException("IncidentId must not be empty.", nameof(incidentId));

        if (authorEmployeeId == Guid.Empty)
            throw new ArgumentException("AuthorEmployeeId must not be empty.", nameof(authorEmployeeId));

        ArgumentException.ThrowIfNullOrWhiteSpace(content, nameof(content));

        return new IncidentComment
        {
            Id = Guid.NewGuid(),
            IncidentId = incidentId,
            AuthorEmployeeId = authorEmployeeId,
            Content = content.Trim()
        };
    }
}
