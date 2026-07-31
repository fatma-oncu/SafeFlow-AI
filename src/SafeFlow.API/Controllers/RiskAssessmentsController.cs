using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeFlow.API.Authorization;
using SafeFlow.API.Extensions;
using SafeFlow.Application.RiskAssessments.Commands.AddControlMeasure;
using SafeFlow.Application.RiskAssessments.Commands.AddHazard;
using SafeFlow.Application.RiskAssessments.Commands.ApproveRiskAssessment;
using SafeFlow.Application.RiskAssessments.Commands.ArchiveRiskAssessment;
using SafeFlow.Application.RiskAssessments.Commands.CreateRevision;
using SafeFlow.Application.RiskAssessments.Commands.CreateRiskAssessment;
using SafeFlow.Application.RiskAssessments.Commands.DeleteRiskAssessment;
using SafeFlow.Application.RiskAssessments.Commands.RejectRiskAssessment;
using SafeFlow.Application.RiskAssessments.Commands.RemoveControlMeasure;
using SafeFlow.Application.RiskAssessments.Commands.RemoveHazard;
using SafeFlow.Application.RiskAssessments.Commands.SubmitForReview;
using SafeFlow.Application.RiskAssessments.Commands.UpdateRiskAssessment;
using SafeFlow.Application.RiskAssessments.DTOs;
using SafeFlow.Application.RiskAssessments.Queries.GetRiskAssessmentById;
using SafeFlow.Application.RiskAssessments.Queries.GetRiskAssessments;
using SafeFlow.Application.RiskAssessments.Queries.SearchRiskAssessments;
using SafeFlow.Domain.RiskAssessments.Enums;
using SafeFlow.SharedKernel.Results;
using static SafeFlow.Application.RiskAssessments.DTOs.RiskAssessmentRequests;

namespace SafeFlow.API.Controllers;

/// <summary>
/// Risk Assessment management endpoints.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/risk-assessments")]
[Produces("application/json")]
[Authorize]
public sealed class RiskAssessmentsController : ApiControllerBase
{
    /// <summary>Retrieves a paged list of risk assessments.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.RiskRead)]
    [ProducesResponseType(typeof(PagedResult<RiskAssessmentSearchResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] AssessmentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRiskAssessmentsQuery(page, pageSize, departmentId, status);
        var result = await Mediator.Send(query, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Performs multi-criterion search and filtering across risk assessments.</summary>
    [HttpGet("search")]
    [Authorize(Policy = Permissions.RiskRead)]
    [ProducesResponseType(typeof(PagedResult<RiskAssessmentSearchResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? q = null,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] Guid? responsibleEmployeeId = null,
        [FromQuery] Guid? createdByEmployeeId = null,
        [FromQuery] string? assessmentNumber = null,
        [FromQuery] AssessmentStatus? status = null,
        [FromQuery] RiskLevel? riskLevel = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchRiskAssessmentsQuery(
            q,
            departmentId,
            responsibleEmployeeId,
            createdByEmployeeId,
            assessmentNumber,
            status,
            riskLevel,
            fromDate,
            toDate,
            page,
            pageSize);

        var result = await Mediator.Send(query, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Retrieves a risk assessment by ID.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.RiskRead)]
    [ProducesResponseType(typeof(RiskAssessmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetRiskAssessmentByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Creates a new risk assessment in Draft status.</summary>
    [HttpPost]
    [Authorize(Policy = Permissions.RiskCreate)]
    [ProducesResponseType(typeof(RiskAssessmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateRiskAssessmentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateRiskAssessmentCommand(
            request.Title,
            request.Description,
            request.DepartmentId,
            request.CreatedByEmployeeId,
            request.ResponsibleEmployeeId,
            request.TenantId,
            request.NextReviewDate);

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

    /// <summary>Creates a new revision of an existing risk assessment.</summary>
    [HttpPost("{id:guid}/revisions")]
    [Authorize(Policy = Permissions.RiskCreate)]
    [ProducesResponseType(typeof(RiskAssessmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateRevision(
        Guid id,
        [FromBody] CreateRevisionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateRevisionCommand(id, request.CreatedByEmployeeId);
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

    /// <summary>Updates risk assessment header details with optimistic concurrency check.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.RiskUpdate)]
    [ProducesResponseType(typeof(RiskAssessmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateRiskAssessmentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateRiskAssessmentCommand(
            id,
            request.Title,
            request.Description,
            request.DepartmentId,
            request.ResponsibleEmployeeId,
            request.NextReviewDate,
            request.RowVersion);

        var result = await Mediator.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Submits a draft risk assessment for approval review.</summary>
    [HttpPost("{id:guid}/submit")]
    [Authorize(Policy = Permissions.RiskUpdate)]
    [ProducesResponseType(typeof(RiskAssessmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitForReview(
        Guid id,
        [FromQuery] Guid submittedByEmployeeId,
        CancellationToken cancellationToken)
    {
        var command = new SubmitForReviewCommand(id, submittedByEmployeeId);
        var result = await Mediator.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Approves a risk assessment in review.</summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = Permissions.RiskApprove)]
    [ProducesResponseType(typeof(RiskAssessmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Approve(
        Guid id,
        [FromBody] ApproveRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ApproveRiskAssessmentCommand(id, request.ApproverEmployeeId, request.Comment);
        var result = await Mediator.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Rejects a risk assessment in review.</summary>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Policy = Permissions.RiskApprove)]
    [ProducesResponseType(typeof(RiskAssessmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] RejectRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RejectRiskAssessmentCommand(id, request.ReviewerEmployeeId, request.Comment);
        var result = await Mediator.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Archives a risk assessment.</summary>
    [HttpPost("{id:guid}/archive")]
    [Authorize(Policy = Permissions.RiskArchive)]
    [ProducesResponseType(typeof(RiskAssessmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
    {
        var command = new ArchiveRiskAssessmentCommand(id);
        var result = await Mediator.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Soft-deletes a risk assessment.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.RiskDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteRiskAssessmentCommand(id);
        var result = await Mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return result.ToActionResult(this);
        }

        return NoContent();
    }

    /// <summary>Adds a hazard to a risk assessment.</summary>
    [HttpPost("{id:guid}/hazards")]
    [Authorize(Policy = Permissions.RiskUpdate)]
    [ProducesResponseType(typeof(RiskHazardDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddHazard(
        Guid id,
        [FromBody] AddHazardRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddHazardCommand(
            id,
            request.Description,
            request.InitialLikelihood,
            request.InitialSeverity,
            request.ResidualLikelihood,
            request.ResidualSeverity);

        var result = await Mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return result.ToActionResult(this);
        }

        return CreatedAtAction(nameof(GetById), new { version = "1.0", id }, result.Value);
    }

    /// <summary>Removes a hazard from a risk assessment.</summary>
    [HttpDelete("{id:guid}/hazards/{hazardId:guid}")]
    [Authorize(Policy = Permissions.RiskUpdate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveHazard(
        Guid id,
        Guid hazardId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveHazardCommand(id, hazardId);
        var result = await Mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return result.ToActionResult(this);
        }

        return NoContent();
    }

    /// <summary>Adds a control measure to a hazard in a risk assessment.</summary>
    [HttpPost("{id:guid}/hazards/{hazardId:guid}/controls")]
    [Authorize(Policy = Permissions.RiskUpdate)]
    [ProducesResponseType(typeof(RiskControlMeasureDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddControlMeasure(
        Guid id,
        Guid hazardId,
        [FromBody] AddControlMeasureRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddControlMeasureCommand(
            id,
            hazardId,
            request.Description,
            request.Type,
            request.IsImplemented);

        var result = await Mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return result.ToActionResult(this);
        }

        return CreatedAtAction(nameof(GetById), new { version = "1.0", id }, result.Value);
    }

    /// <summary>Removes a control measure from a hazard in a risk assessment.</summary>
    [HttpDelete("{id:guid}/hazards/{hazardId:guid}/controls/{controlId:guid}")]
    [Authorize(Policy = Permissions.RiskUpdate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveControlMeasure(
        Guid id,
        Guid hazardId,
        Guid controlId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveControlMeasureCommand(id, hazardId, controlId);
        var result = await Mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return result.ToActionResult(this);
        }

        return NoContent();
    }
}
