using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Employees;

/// <summary>
/// Machine-readable error definitions for the Employee module.
/// </summary>
public static class EmployeeErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Employee.NotFound",
        "Employee record was not found.");

    public static readonly Error NumberAlreadyExists = Error.Conflict(
        "Employee.NumberAlreadyExists",
        "An employee with this employee number already exists.");

    public static readonly Error EmailAlreadyExists = Error.Conflict(
        "Employee.EmailAlreadyExists",
        "An employee with this email address already exists.");

    public static readonly Error ConcurrencyConflict = Error.Conflict(
        "Employee.ConcurrencyConflict",
        "The employee record was modified by another operation. Please reload and try again.");

    public static readonly Error InvalidDepartment = Error.Validation(
        "Employee.InvalidDepartment",
        "The specified department identifier is invalid.");
}
