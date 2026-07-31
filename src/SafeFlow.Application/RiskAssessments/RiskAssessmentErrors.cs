using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.RiskAssessments;

/// <summary>
/// Domain and application error codes for Risk Assessment operations.
/// </summary>
public static class RiskAssessmentErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "RiskAssessment.NotFound",
        "The requested risk assessment was not found.");

    public static readonly Error NumberAlreadyExists = Error.Conflict(
        "RiskAssessment.NumberAlreadyExists",
        "A risk assessment with the generated number already exists.");

    public static readonly Error DepartmentNotFound = Error.NotFound(
        "RiskAssessment.DepartmentNotFound",
        "The assigned department was not found.");

    public static readonly Error EmployeeNotFound = Error.NotFound(
        "RiskAssessment.EmployeeNotFound",
        "The specified employee (creator or responsible person) was not found.");

    public static readonly Error ArchivedImmutable = Error.Validation(
        "RiskAssessment.ArchivedImmutable",
        "Archived risk assessments cannot be modified.");
}
