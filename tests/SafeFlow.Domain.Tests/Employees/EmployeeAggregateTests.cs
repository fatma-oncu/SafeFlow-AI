using FluentAssertions;
using SafeFlow.Domain.Employees.Aggregates;
using SafeFlow.Domain.Employees.Enums;
using SafeFlow.Domain.Employees.Events;
using SafeFlow.Domain.Employees.ValueObjects;
using SafeFlow.SharedKernel.Exceptions;
using SafeFlow.SharedKernel.ValueObjects;
using Xunit;

namespace SafeFlow.Domain.Tests.Employees;

public class EmployeeAggregateTests
{
    private static Employee CreateTestEmployee()
    {
        return Employee.Create(
            EmployeeNumber.Create("EMP-2026-0001"),
            "John",
            "Doe",
            Email.Create("john.doe@example.com"),
            PhoneNumber.Create("+1234567890"),
            DepartmentId.CreateUnique(),
            JobTitle.Create("Software Engineer"),
            EmploymentType.FullTime,
            DateTime.UtcNow.AddYears(-1),
            Guid.NewGuid());
    }

    [Fact]
    public void Create_WithValidParameters_ShouldInstantiateEmployeeAndRaiseDomainEvent()
    {
        // Act
        var employee = CreateTestEmployee();

        // Assert
        employee.Should().NotBeNull();
        employee.FirstName.Should().Be("John");
        employee.LastName.Should().Be("Doe");
        employee.FullName.Should().Be("John Doe");
        employee.Email.Value.Should().Be("john.doe@example.com");
        employee.EmploymentStatus.Should().Be(EmploymentStatus.Active);
        employee.IsActive.Should().BeTrue();
        employee.DomainEvents.Should().ContainSingle(e => e is EmployeeCreatedDomainEvent);
    }

    [Fact]
    public void Create_WithInvalidFirstName_ShouldThrowValidationException()
    {
        // Act & Assert
        Action act = () => Employee.Create(
            EmployeeNumber.Create("EMP-2026-0001"),
            "",
            "Doe",
            Email.Create("john.doe@example.com"),
            null,
            DepartmentId.CreateUnique(),
            JobTitle.Create("Engineer"),
            EmploymentType.FullTime,
            DateTime.UtcNow,
            Guid.NewGuid());

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void UpdateProfile_WithValidParameters_ShouldUpdateFieldsAndRaiseEvent()
    {
        // Arrange
        var employee = CreateTestEmployee();
        employee.ClearDomainEvents();

        // Act
        employee.UpdateProfile("Jane", "Smith", Email.Create("jane.smith@example.com"), null);

        // Assert
        employee.FirstName.Should().Be("Jane");
        employee.LastName.Should().Be("Smith");
        employee.Email.Value.Should().Be("jane.smith@example.com");
        employee.PhoneNumber.Should().BeNull();
        employee.DomainEvents.Should().ContainSingle(e => e is EmployeeProfileUpdatedDomainEvent);
    }

    [Fact]
    public void DeactivateAndActivate_ShouldChangeStatusAndRaiseEvents()
    {
        // Arrange
        var employee = CreateTestEmployee();
        employee.ClearDomainEvents();

        // Act - Deactivate
        employee.Deactivate();
        employee.IsActive.Should().BeFalse();
        employee.EmploymentStatus.Should().Be(EmploymentStatus.Inactive);
        employee.DomainEvents.Should().ContainSingle(e => e is EmployeeDeactivatedDomainEvent);

        employee.ClearDomainEvents();

        // Act - Activate
        employee.Activate();
        employee.IsActive.Should().BeTrue();
        employee.EmploymentStatus.Should().Be(EmploymentStatus.Active);
        employee.DomainEvents.Should().ContainSingle(e => e is EmployeeActivatedDomainEvent);
    }

    [Fact]
    public void TransferDepartment_WithNewDepartment_ShouldUpdateDepartmentAndRaiseEvent()
    {
        // Arrange
        var employee = CreateTestEmployee();
        var newDept = DepartmentId.CreateUnique();
        employee.ClearDomainEvents();

        // Act
        employee.TransferDepartment(newDept);

        // Assert
        employee.DepartmentId.Should().Be(newDept);
        employee.DomainEvents.Should().ContainSingle(e => e is EmployeeTransferredDomainEvent);
    }

    [Fact]
    public void ChangeJobTitle_WithNewTitle_ShouldUpdateTitleAndRaiseEvent()
    {
        // Arrange
        var employee = CreateTestEmployee();
        var newTitle = JobTitle.Create("Senior Software Engineer");
        employee.ClearDomainEvents();

        // Act
        employee.ChangeJobTitle(newTitle);

        // Assert
        employee.JobTitle.Should().Be(newTitle);
        employee.DomainEvents.Should().ContainSingle(e => e is EmployeeJobTitleChangedDomainEvent);
    }

    [Fact]
    public void SoftDelete_ShouldSetIsDeletedAndTerminatedStatus()
    {
        // Arrange
        var employee = CreateTestEmployee();
        employee.ClearDomainEvents();

        // Act
        employee.PerformSoftDelete("admin-user");

        // Assert
        employee.IsDeleted.Should().BeTrue();
        employee.DeletedBy.Should().Be("admin-user");
        employee.DeletedAt.Should().NotBeNull();
        employee.IsActive.Should().BeFalse();
        employee.EmploymentStatus.Should().Be(EmploymentStatus.Terminated);
        employee.DomainEvents.Should().ContainSingle(e => e is EmployeeSoftDeletedDomainEvent);
    }

    [Fact]
    public void LinkUser_And_UnlinkUser_ShouldUpdateUserId()
    {
        // Arrange
        var employee = CreateTestEmployee();
        var userId = Guid.NewGuid();

        // Act
        employee.LinkUser(userId);
        employee.UserId.Should().Be(userId);

        employee.UnlinkUser();
        employee.UserId.Should().BeNull();
    }
}
