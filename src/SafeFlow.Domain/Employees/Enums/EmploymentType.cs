namespace SafeFlow.Domain.Employees.Enums;

/// <summary>
/// Defines the employment classification of an employee.
/// </summary>
public enum EmploymentType
{
    /// <summary>Permanent full-time employee.</summary>
    FullTime = 1,

    /// <summary>Part-time employee.</summary>
    PartTime = 2,

    /// <summary>External contractor or consultant.</summary>
    Contractor = 3,

    /// <summary>Trainee or intern.</summary>
    Intern = 4,
}
