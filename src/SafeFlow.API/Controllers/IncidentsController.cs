using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeFlow.API.Authorization;
using SafeFlow.API.Extensions;
using SafeFlow.Application.Incidents.Commands;
using SafeFlow.Application.Incidents.Commands.CreateIncident;
using SafeFlow.Application.Incidents.Commands.UpdateIncident;
using SafeFlow.Application.Incidents.DTOs;
using SafeFlow.Application.Incidents.Queries.GetIncidentById;
using SafeFlow.Application.Incidents.Queries.GetIncidents;
using SafeFlow.Application.Incidents.Queries.SearchIncidents;
using SafeFlow.Domain.Incidents.Enums;
using SafeFlow.SharedKernel.Results;
using static SafeFlow.Application.Incidents.DTOs.IncidentRequests;

namespace SafeFlow.API.Controllers;

/// <summary>
/// Workplace Incident management endpoints.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/incidents")]
[Produces("application/json")]
[Authorize]
public sealed class IncidentsController : ApiControllerBase
{
    /// <summary>Retrieves a paged list of incidents.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.IncidentRead)]
    [ProducesResponseType(typeof(PagedResult<IncidentSearchResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] IncidentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetIncidentsQuery(page, pageSize, departmentId, status);
        var result = await Mediator.Send(query, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Performs multi-criterion search across incidents.</summary>
    [HttpGet("search")]
    [Authorize(Policy = Permissions.IncidentRead)]
    [ProducesResponseType(typeof(PagedResult<IncidentSearchResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? q = null,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] Guid? reportedByEmployeeId = null,
        [FromQuery] Guid? assignedToEmployeeId = null,
        [FromQuery] IncidentStatus? status = null,
        [FromQuery] IncidentSeverity? severity = null,
        [FromQuery] IncidentCategory? category = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchIncidentsQuery(
            q,
            departmentId,
            reportedByEmployeeId,
            assignedToEmployeeId,
            status,
            severity,
            category,
            fromDate,
            toDate,
            page,
            pageSize);

        var result = await Mediator.Send(query, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Retrieves an incident by ID.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.IncidentRead)]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetIncidentByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Reports a new incident.</summary>
    [HttpPost]
    [Authorize(Policy = Permissions.IncidentCreate)]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateIncidentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateIncidentCommand(
            request.Title,
            request.Description,
            request.Location,
            request.Severity,
            request.Category,
            request.OccurredAt,
            request.DepartmentId,
            request.ReportedByEmployeeId,
            request.TenantId,
            request.RiskAssessmentId,
            request.AffectedEmployeeId);

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

    /// <summary>Updates incident header details with optimistic concurrency check.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.IncidentUpdate)]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateIncidentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateIncidentCommand(
            id,
            request.Title,
            request.Description,
            request.Location,
            request.Severity,
            request.Category,
            request.OccurredAt,
            request.DepartmentId,
            request.RiskAssessmentId,
            request.AffectedEmployeeId,
            request.RowVersion);

        var result = await Mediator.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Soft-deletes an incident.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.IncidentDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteIncidentCommand(id);
        var result = await Mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return result.ToActionResult(this);
        }

        return NoContent();
    }

    /// <summary>Assigns an incident to an investigator.</summary>
    [HttpPost("{id:guid}/assign")]
    [Authorize(Policy = Permissions.IncidentAssign)]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Assign(
        Guid id,
        [FromBody] AssignIncidentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AssignIncidentCommand(id, request.AssignedToEmployeeId);
        var result = await Mediator.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Starts formal investigation on an incident.</summary>
    [HttpPost("{id:guid}/start-investigation")]
    [Authorize(Policy = Permissions.IncidentInvestigate)]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StartInvestigation(
        Guid id,
        [FromBody] StartInvestigationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new StartInvestigationCommand(id, request.InvestigatorEmployeeId);
        var result = await Mediator.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Adds a comment to an incident.</summary>
    [HttpPost("{id:guid}/comments")]
    [Authorize(Policy = Permissions.IncidentUpdate)]
    [ProducesResponseType(typeof(IncidentCommentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddComment(
        Guid id,
        [FromBody] AddCommentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddCommentCommand(id, request.AuthorEmployeeId, request.Content);
        var result = await Mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return result.ToActionResult(this);
        }

        return CreatedAtAction(nameof(GetById), new { version = "1.0", id }, result.Value);
    }

    /// <summary>Adds an attachment to an incident.</summary>
    [HttpPost("{id:guid}/attachments")]
    [Authorize(Policy = Permissions.IncidentUpdate)]
    [ProducesResponseType(typeof(IncidentAttachmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddAttachment(
        Guid id,
        [FromBody] AddAttachmentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddAttachmentCommand(
            id,
            request.FileName,
            request.FileUrl,
            request.ContentType,
            request.SizeBytes,
            request.UploadedByEmployeeId);

        var result = await Mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return result.ToActionResult(this);
        }

        return CreatedAtAction(nameof(GetById), new { version = "1.0", id }, result.Value);
    }

    /// <summary>Adds a corrective action to an incident.</summary>
    [HttpPost("{id:guid}/corrective-actions")]
    [Authorize(Policy = Permissions.IncidentUpdate)]
    [ProducesResponseType(typeof(CorrectiveActionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddCorrectiveAction(
        Guid id,
        [FromBody] AddCorrectiveActionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddCorrectiveActionCommand(id, request.Description, request.AssignedToEmployeeId, request.DueDate);
        var result = await Mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return result.ToActionResult(this);
        }

        return CreatedAtAction(nameof(GetById), new { version = "1.0", id }, result.Value);
    }

    /// <summary>Marks a corrective action as completed.</summary>
    [HttpPost("{id:guid}/corrective-actions/{actionId:guid}/complete")]
    [Authorize(Policy = Permissions.IncidentUpdate)]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteCorrectiveAction(
        Guid id,
        Guid actionId,
        [FromBody] CompleteCorrectiveActionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CompleteCorrectiveActionCommand(id, actionId, request.CompletedByEmployeeId);
        var result = await Mediator.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Resolves an incident after investigation.</summary>
    [HttpPost("{id:guid}/resolve")]
    [Authorize(Policy = Permissions.IncidentResolve)]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Resolve(
        Guid id,
        [FromBody] ResolveIncidentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ResolveIncidentCommand(id, request.InvestigationResult, request.ResolutionSummary);
        var result = await Mediator.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Formally closes a resolved incident.</summary>
    [HttpPost("{id:guid}/close")]
    [Authorize(Policy = Permissions.IncidentClose)]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Close(
        Guid id,
        [FromBody] CloseIncidentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CloseIncidentCommand(id, request.ClosedByEmployeeId, request.ClosureNotes);
        var result = await Mediator.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Cancels an incident report.</summary>
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = Permissions.IncidentUpdate)]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(
        Guid id,
        [FromBody] CancelIncidentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CancelIncidentCommand(id, request.Reason);
        var result = await Mediator.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Reopens a closed or cancelled incident.</summary>
    [HttpPost("{id:guid}/reopen")]
    [Authorize(Policy = Permissions.IncidentUpdate)]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reopen(
        Guid id,
        [FromBody] ReopenIncidentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ReopenIncidentCommand(id, request.Reason);
        var result = await Mediator.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }
}
