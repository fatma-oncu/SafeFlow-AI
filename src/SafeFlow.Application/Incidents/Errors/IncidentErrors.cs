using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Incidents.Errors;

/// <summary>
/// Domain and Application errors for Incident use cases.
/// </summary>
public static class IncidentErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Incident.NotFound", "The requested incident was not found.");

    public static readonly Error NumberAlreadyExists = Error.Conflict(
        "Incident.NumberAlreadyExists", "An incident with the generated number already exists.");

    public static readonly Error EmployeeNotFound = Error.NotFound(
        "Incident.EmployeeNotFound", "One or more referenced employee records were not found.");

    public static readonly Error RiskAssessmentNotFound = Error.NotFound(
        "Incident.RiskAssessmentNotFound", "The referenced risk assessment was not found.");

    public static readonly Error ConcurrencyConflict = Error.Conflict(
        "Incident.ConcurrencyConflict", "The incident record has been modified by another user. Please reload and try again.");

    public static readonly Error InvalidStatusTransition = Error.Validation(
        "Incident.InvalidStatusTransition", "The requested status transition is not permitted.");
}
