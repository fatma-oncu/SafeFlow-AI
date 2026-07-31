using SafeFlow.Domain.Incidents.Aggregates;
using SafeFlow.Domain.Incidents.Enums;
using SafeFlow.SharedKernel.Specifications;

namespace SafeFlow.Application.Incidents.Specifications;

/// <summary>Specification for loading an Incident by ID with eager loading of attachments, comments, and corrective actions.</summary>
public sealed class IncidentByIdSpecification : BaseSpecification<Incident>
{
    public IncidentByIdSpecification(Guid id)
        : base(i => i.Id == id)
    {
        AddInclude(i => i.Attachments);
        AddInclude(i => i.Comments);
        AddInclude(i => i.CorrectiveActions);
    }
}

/// <summary>Specification for loading an Incident by unique IncidentNumber.</summary>
public sealed class IncidentByNumberSpecification : BaseSpecification<Incident>
{
    public IncidentByNumberSpecification(string number)
        : base(i => i.IncidentNumber.Value == number.Trim().ToUpperInvariant())
    {
        AddInclude(i => i.Attachments);
        AddInclude(i => i.Comments);
        AddInclude(i => i.CorrectiveActions);
    }
}

/// <summary>Specification for paged list of Incidents with optional department or status filter.</summary>
public sealed class IncidentPagedSpecification : BaseSpecification<Incident>
{
    public IncidentPagedSpecification(int page, int pageSize, Guid? departmentId = null, IncidentStatus? status = null)
        : base(i => (!departmentId.HasValue || i.DepartmentId == departmentId.Value) &&
                    (!status.HasValue || i.Status == status.Value))
    {
        ApplyOrderByDescending(i => i.OccurredAt);
        ApplyPaging((page - 1) * pageSize, pageSize);
        ApplyNoTracking();
    }
}

/// <summary>Specification for multi-criterion search across Incidents.</summary>
public sealed class IncidentSearchSpecification : BaseSpecification<Incident>
{
    public IncidentSearchSpecification(
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
        : base(i =>
            (string.IsNullOrWhiteSpace(searchTerm) ||
                i.IncidentNumber.Value.Contains(searchTerm.Trim()) ||
                i.Title.Value.Contains(searchTerm.Trim()) ||
                i.Description.Value.Contains(searchTerm.Trim()) ||
                i.Location.Value.Contains(searchTerm.Trim())) &&
            (!departmentId.HasValue || i.DepartmentId == departmentId.Value) &&
            (!reportedByEmployeeId.HasValue || i.ReportedByEmployeeId == reportedByEmployeeId.Value) &&
            (!assignedToEmployeeId.HasValue || i.AssignedToEmployeeId == assignedToEmployeeId.Value) &&
            (!status.HasValue || i.Status == status.Value) &&
            (!severity.HasValue || i.Severity == severity.Value) &&
            (!category.HasValue || i.Category == category.Value) &&
            (!fromDate.HasValue || i.OccurredAt >= fromDate.Value) &&
            (!toDate.HasValue || i.OccurredAt <= toDate.Value))
    {
        ApplyOrderByDescending(i => i.OccurredAt);
        ApplyPaging((page - 1) * pageSize, pageSize);
        ApplyNoTracking();
    }
}

/// <summary>Specification for counting total search results without paging.</summary>
public sealed class IncidentSearchCountSpecification : BaseSpecification<Incident>
{
    public IncidentSearchCountSpecification(
        string? searchTerm = null,
        Guid? departmentId = null,
        Guid? reportedByEmployeeId = null,
        Guid? assignedToEmployeeId = null,
        IncidentStatus? status = null,
        IncidentSeverity? severity = null,
        IncidentCategory? category = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
        : base(i =>
            (string.IsNullOrWhiteSpace(searchTerm) ||
                i.IncidentNumber.Value.Contains(searchTerm.Trim()) ||
                i.Title.Value.Contains(searchTerm.Trim()) ||
                i.Description.Value.Contains(searchTerm.Trim()) ||
                i.Location.Value.Contains(searchTerm.Trim())) &&
            (!departmentId.HasValue || i.DepartmentId == departmentId.Value) &&
            (!reportedByEmployeeId.HasValue || i.ReportedByEmployeeId == reportedByEmployeeId.Value) &&
            (!assignedToEmployeeId.HasValue || i.AssignedToEmployeeId == assignedToEmployeeId.Value) &&
            (!status.HasValue || i.Status == status.Value) &&
            (!severity.HasValue || i.Severity == severity.Value) &&
            (!category.HasValue || i.Category == category.Value) &&
            (!fromDate.HasValue || i.OccurredAt >= fromDate.Value) &&
            (!toDate.HasValue || i.OccurredAt <= toDate.Value))
    {
        ApplyNoTracking();
    }
}

/// <summary>Specification for filtering incidents by Status.</summary>
public sealed class IncidentByStatusSpecification : BaseSpecification<Incident>
{
    public IncidentByStatusSpecification(IncidentStatus status)
        : base(i => i.Status == status)
    {
        ApplyOrderByDescending(i => i.OccurredAt);
        ApplyNoTracking();
    }
}

/// <summary>Specification for filtering incidents by Employee (reported, assigned, or affected).</summary>
public sealed class IncidentByEmployeeSpecification : BaseSpecification<Incident>
{
    public IncidentByEmployeeSpecification(Guid employeeId)
        : base(i => i.ReportedByEmployeeId == employeeId ||
                    i.AssignedToEmployeeId == employeeId ||
                    i.AffectedEmployeeId == employeeId)
    {
        ApplyOrderByDescending(i => i.OccurredAt);
        ApplyNoTracking();
    }
}

/// <summary>Specification for fetching all open (non-closed, non-cancelled) incidents.</summary>
public sealed class OpenIncidentsSpecification : BaseSpecification<Incident>
{
    public OpenIncidentsSpecification()
        : base(i => i.Status != IncidentStatus.Closed && i.Status != IncidentStatus.Cancelled)
    {
        ApplyOrderByDescending(i => i.OccurredAt);
        ApplyNoTracking();
    }
}
