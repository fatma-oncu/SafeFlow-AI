namespace SafeFlow.Domain.Employees.Enums;

/// <summary>
/// Defines the current operational status of an employee.
/// </summary>
public enum EmploymentStatus
{
    /// <summary>Employee is actively employed and working.</summary>
    Active = 1,

    /// <summary>Employee is inactive (e.g. suspended).</summary>
    Inactive = 2,

    /// <summary>Employment has been terminated.</summary>
    Terminated = 3,

    /// <summary>Employee is on an extended leave of absence.</summary>
    OnLeave = 4,
}
