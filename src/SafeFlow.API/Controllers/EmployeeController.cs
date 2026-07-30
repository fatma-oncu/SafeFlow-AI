using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeFlow.API.Authorization;
using SafeFlow.API.Extensions;
using SafeFlow.Application.Employees.Commands.ActivateEmployee;
using SafeFlow.Application.Employees.Commands.CreateEmployee;
using SafeFlow.Application.Employees.Commands.DeactivateEmployee;
using SafeFlow.Application.Employees.Commands.DeleteEmployee;
using SafeFlow.Application.Employees.Commands.TransferEmployee;
using SafeFlow.Application.Employees.Commands.UpdateEmployee;
using SafeFlow.Application.Employees.DTOs;
using SafeFlow.Application.Employees.Queries.GetEmployeeById;
using SafeFlow.Application.Employees.Queries.GetEmployees;
using SafeFlow.Application.Employees.Queries.SearchEmployees;
using SafeFlow.SharedKernel.Results;
using static SafeFlow.Application.Employees.DTOs.EmployeeRequests;

namespace SafeFlow.API.Controllers;

/// <summary>
/// Employee management endpoints.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/employees")]
[Produces("application/json")]
[Authorize]
public sealed class EmployeeController : ApiControllerBase
{
    /// <summary>Creates a new employee record.</summary>
    [HttpPost]
    [Authorize(Policy = Permissions.EmployeesCreate)]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateEmployeeCommand(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.DepartmentId,
            request.JobTitle,
            request.EmploymentType,
            request.HireDate,
            request.TenantId,
            request.UserId);

        var result = await Mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return result.ToActionResult(this);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { version = "1.0", id = result.Value.Id },
            result.Value);
    }

    /// <summary>Returns a paged list of employees.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.EmployeesRead)]
    [ProducesResponseType(typeof(PagedResult<EmployeeListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetEmployeesQuery(page, pageSize, departmentId, isActive);
        var result = await Mediator.Send(query, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Searches employees with search criteria and paging.</summary>
    [HttpGet("search")]
    [Authorize(Policy = Permissions.EmployeesRead)]
    [ProducesResponseType(typeof(PagedResult<EmployeeSearchResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? q = null,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchEmployeesQuery(q, departmentId, isActive, page, pageSize);
        var result = await Mediator.Send(query, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Gets an employee by unique identifier.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.EmployeesRead)]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetEmployeeByIdQuery(id), cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Updates an employee profile.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.EmployeesUpdate)]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateEmployeeCommand(
            id,
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.JobTitle,
            request.RowVersion);

        var result = await Mediator.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Soft-deletes an employee record.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.EmployeesDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new DeleteEmployeeCommand(id), cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Activates an employee record.</summary>
    [HttpPut("{id:guid}/activate")]
    [Authorize(Policy = Permissions.EmployeesUpdate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new ActivateEmployeeCommand(id), cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Deactivates an employee record.</summary>
    [HttpPut("{id:guid}/deactivate")]
    [Authorize(Policy = Permissions.EmployeesUpdate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new DeactivateEmployeeCommand(id), cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Transfers an employee to a new department.</summary>
    [HttpPut("{id:guid}/transfer")]
    [Authorize(Policy = Permissions.EmployeesTransfer)]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Transfer(
        [FromRoute] Guid id,
        [FromBody] TransferEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var command = new TransferEmployeeCommand(id, request.NewDepartmentId, request.RowVersion);
        var result = await Mediator.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }
}
