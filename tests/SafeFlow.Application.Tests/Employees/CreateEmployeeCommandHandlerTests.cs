using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SafeFlow.Application.Employees;
using SafeFlow.Application.Employees.Commands.CreateEmployee;
using SafeFlow.Application.Employees.Interfaces;
using SafeFlow.Domain.Employees.Aggregates;
using SafeFlow.Domain.Employees.Enums;
using SafeFlow.Domain.Employees.ValueObjects;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.ValueObjects;
using Xunit;

namespace SafeFlow.Application.Tests.Employees;

public class CreateEmployeeCommandHandlerTests
{
    private readonly IRepository<Employee> _repository = Substitute.For<IRepository<Employee>>();
    private readonly IEmployeeNumberGenerator _numberGenerator = Substitute.For<IEmployeeNumberGenerator>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ILogger<CreateEmployeeCommandHandler> _logger = Substitute.For<ILogger<CreateEmployeeCommandHandler>>();
    private readonly CreateEmployeeCommandHandler _handler;

    public CreateEmployeeCommandHandlerTests()
    {
        _handler = new CreateEmployeeCommandHandler(_repository, _numberGenerator, _unitOfWork, _logger);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateEmployeeAndReturnSuccessResult()
    {
        // Arrange
        var command = new CreateEmployeeCommand(
            "John",
            "Doe",
            "john.doe@example.com",
            "+1234567890",
            Guid.NewGuid(),
            "Software Engineer",
            EmploymentType.FullTime,
            DateTime.UtcNow,
            Guid.NewGuid());

        _numberGenerator.GenerateNextNumberAsync(Arg.Any<CancellationToken>())
            .Returns("EMP-2026-0001");

        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<Employee>>(), Arg.Any<CancellationToken>())
            .Returns((Employee?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.FirstName.Should().Be("John");
        result.Value.LastName.Should().Be("Doe");
        result.Value.EmployeeNumber.Should().Be("EMP-2026-0001");

        await _repository.Received(1).AddAsync(Arg.Any<Employee>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new CreateEmployeeCommand(
            "John",
            "Doe",
            "existing@example.com",
            null,
            Guid.NewGuid(),
            "Engineer",
            EmploymentType.FullTime,
            DateTime.UtcNow,
            Guid.NewGuid());

        var existingEmp = Employee.Create(
            EmployeeNumber.Create("EMP-2026-0002"),
            "Existing",
            "User",
            Email.Create("existing@example.com"),
            null,
            DepartmentId.CreateUnique(),
            JobTitle.Create("Engineer"),
            EmploymentType.FullTime,
            DateTime.UtcNow,
            command.TenantId);

        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<Employee>>(), Arg.Any<CancellationToken>())
            .Returns(existingEmp);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EmployeeErrors.EmailAlreadyExists);
    }
}
