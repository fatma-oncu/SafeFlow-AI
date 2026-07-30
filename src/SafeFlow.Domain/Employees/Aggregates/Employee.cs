using SafeFlow.Domain.Employees.Enums;
using SafeFlow.Domain.Employees.Events;
using SafeFlow.Domain.Employees.ValueObjects;
using SafeFlow.SharedKernel.Entities;
using SafeFlow.SharedKernel.Exceptions;
using SafeFlow.SharedKernel.ValueObjects;

namespace SafeFlow.Domain.Employees.Aggregates;

/// <summary>
/// Aggregate Root representing an Employee within the enterprise domain.
/// </summary>
public sealed class Employee : AggregateRoot
{
    /// <summary>Private parameterless constructor for ORM materialization.</summary>
    private Employee() { }

    // ── Properties ───────────────────────────────────────────────────────────

    /// <summary>Gets the unique employee number.</summary>
    public EmployeeNumber EmployeeNumber { get; private set; } = default!;

    /// <summary>Gets the first name.</summary>
    public string FirstName { get; private set; } = default!;

    /// <summary>Gets the last name.</summary>
    public string LastName { get; private set; } = default!;

    /// <summary>Gets the formatted full name.</summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>Gets the email address.</summary>
    public Email Email { get; private set; } = default!;

    /// <summary>Gets the optional phone number.</summary>
    public PhoneNumber? PhoneNumber { get; private set; }

    /// <summary>Gets the assigned department identifier.</summary>
    public DepartmentId DepartmentId { get; private set; } = default!;

    /// <summary>Gets the job title.</summary>
    public JobTitle JobTitle { get; private set; } = default!;

    /// <summary>Gets the employment type.</summary>
    public EmploymentType EmploymentType { get; private set; }

    /// <summary>Gets the current employment status.</summary>
    public EmploymentStatus EmploymentStatus { get; private set; }

    /// <summary>Gets the date of hire.</summary>
    public DateTime HireDate { get; private set; }

    /// <summary>Gets the optional referenced Identity User identifier.</summary>
    public Guid? UserId { get; private set; }

    /// <summary>Gets whether the employee is active.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Gets the concurrency token (row version).</summary>
    public byte[] RowVersion { get; private set; } = [];

    // ── Factory Method ───────────────────────────────────────────────────────

    /// <summary>
    /// Factory method to create a new <see cref="Employee"/> aggregate root.
    /// </summary>
    public static Employee Create(
        EmployeeNumber employeeNumber,
        string firstName,
        string lastName,
        Email email,
        PhoneNumber? phoneNumber,
        DepartmentId departmentId,
        JobTitle jobTitle,
        EmploymentType employmentType,
        DateTime hireDate,
        Guid tenantId,
        Guid? userId = null)
    {
        ArgumentNullException.ThrowIfNull(employeeNumber, nameof(employeeNumber));
        ArgumentNullException.ThrowIfNull(email, nameof(email));
        ArgumentNullException.ThrowIfNull(departmentId, nameof(departmentId));
        ArgumentNullException.ThrowIfNull(jobTitle, nameof(jobTitle));

        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(firstName))
            errors[nameof(firstName)] = ["First name is required."];
        else if (firstName.Trim().Length > 100)
            errors[nameof(firstName)] = ["First name must not exceed 100 characters."];

        if (string.IsNullOrWhiteSpace(lastName))
            errors[nameof(lastName)] = ["Last name is required."];
        else if (lastName.Trim().Length > 100)
            errors[nameof(lastName)] = ["Last name must not exceed 100 characters."];

        if (tenantId == Guid.Empty)
            errors[nameof(tenantId)] = ["Tenant identifier must not be empty."];

        if (errors.Count > 0)
            throw new ValidationException(errors);

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            EmployeeNumber = employeeNumber,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email,
            PhoneNumber = phoneNumber,
            DepartmentId = departmentId,
            JobTitle = jobTitle,
            EmploymentType = employmentType,
            EmploymentStatus = EmploymentStatus.Active,
            HireDate = hireDate,
            TenantId = tenantId,
            UserId = userId,
            IsActive = true
        };

        employee.RaiseDomainEvent(new EmployeeCreatedDomainEvent(
            employee.Id,
            employee.EmployeeNumber.Value,
            employee.Email.Value,
            employee.DepartmentId.Value,
            DateTime.UtcNow));

        return employee;
    }

    // ── Domain Methods ───────────────────────────────────────────────────────

    /// <summary>Updates personal details of the employee profile.</summary>
    public void UpdateProfile(string firstName, string lastName, Email email, PhoneNumber? phoneNumber)
    {
        ArgumentNullException.ThrowIfNull(email, nameof(email));

        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(firstName))
            errors[nameof(firstName)] = ["First name is required."];
        else if (firstName.Trim().Length > 100)
            errors[nameof(firstName)] = ["First name must not exceed 100 characters."];

        if (string.IsNullOrWhiteSpace(lastName))
            errors[nameof(lastName)] = ["Last name is required."];
        else if (lastName.Trim().Length > 100)
            errors[nameof(lastName)] = ["Last name must not exceed 100 characters."];

        if (errors.Count > 0)
            throw new ValidationException(errors);

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email;
        PhoneNumber = phoneNumber;

        RaiseDomainEvent(new EmployeeProfileUpdatedDomainEvent(
            Id,
            FirstName,
            LastName,
            Email.Value,
            DateTime.UtcNow));
    }

    /// <summary>Activates the employee record.</summary>
    public void Activate()
    {
        if (IsActive && EmploymentStatus == EmploymentStatus.Active)
            return;

        IsActive = true;
        EmploymentStatus = EmploymentStatus.Active;

        RaiseDomainEvent(new EmployeeActivatedDomainEvent(Id, DateTime.UtcNow));
    }

    /// <summary>Deactivates the employee record.</summary>
    public void Deactivate()
    {
        if (!IsActive && EmploymentStatus == EmploymentStatus.Inactive)
            return;

        IsActive = false;
        EmploymentStatus = EmploymentStatus.Inactive;

        RaiseDomainEvent(new EmployeeDeactivatedDomainEvent(Id, DateTime.UtcNow));
    }

    /// <summary>Transfers the employee to a new department.</summary>
    public void TransferDepartment(DepartmentId newDepartmentId)
    {
        ArgumentNullException.ThrowIfNull(newDepartmentId, nameof(newDepartmentId));

        if (DepartmentId == newDepartmentId)
            return;

        var oldDeptId = DepartmentId.Value;
        DepartmentId = newDepartmentId;

        RaiseDomainEvent(new EmployeeTransferredDomainEvent(
            Id,
            oldDeptId,
            DepartmentId.Value,
            DateTime.UtcNow));
    }

    /// <summary>Changes the employee's job title.</summary>
    public void ChangeJobTitle(JobTitle newJobTitle)
    {
        ArgumentNullException.ThrowIfNull(newJobTitle, nameof(newJobTitle));

        if (JobTitle == newJobTitle)
            return;

        var oldTitle = JobTitle.Value;
        JobTitle = newJobTitle;

        RaiseDomainEvent(new EmployeeJobTitleChangedDomainEvent(
            Id,
            oldTitle,
            JobTitle.Value,
            DateTime.UtcNow));
    }

    /// <summary>Soft deletes the employee record.</summary>
    public void PerformSoftDelete(string deletedBy)
    {
        if (IsDeleted)
            return;

        SoftDelete(string.IsNullOrWhiteSpace(deletedBy) ? "system" : deletedBy);
        IsActive = false;
        EmploymentStatus = EmploymentStatus.Terminated;

        RaiseDomainEvent(new EmployeeSoftDeletedDomainEvent(Id, DateTime.UtcNow));
    }

    /// <summary>Links an Identity User to this employee record.</summary>
    public void LinkUser(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(userId)] = ["User identifier must not be empty."]
            });

        UserId = userId;
    }

    /// <summary>Unlinks the Identity User from this employee record.</summary>
    public void UnlinkUser()
    {
        UserId = null;
    }
}
