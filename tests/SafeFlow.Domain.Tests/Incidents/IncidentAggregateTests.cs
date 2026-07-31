using FluentAssertions;
using SafeFlow.Domain.Incidents.Aggregates;
using SafeFlow.Domain.Incidents.Enums;
using SafeFlow.Domain.Incidents.ValueObjects;
using SafeFlow.SharedKernel.Exceptions;
using Xunit;

namespace SafeFlow.Domain.Tests.Incidents;

public sealed class IncidentAggregateTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldInstantiateInReportedStatus()
    {
        var number = IncidentNumber.Create("INC-2026-000001");
        var title = IncidentTitle.Create("Chemical Spill");
        var description = IncidentDescription.Create("Minor chemical release in storage room B");
        var location = IncidentLocation.Create("Storage Room B, Floor 1");
        var deptId = Guid.NewGuid();
        var empId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var incident = Incident.Create(
            number, title, description, location,
            IncidentSeverity.Moderate, IncidentCategory.Environmental,
            DateTime.UtcNow, deptId, empId, tenantId);

        incident.Should().NotBeNull();
        incident.IncidentNumber.Value.Should().Be("INC-2026-000001");
        incident.Status.Should().Be(IncidentStatus.Reported);
        incident.DomainEvents.Should().ContainSingle(e => e.GetType().Name == "IncidentReportedDomainEvent");
    }

    [Fact]
    public void WorkflowLifecycle_ShouldTransitionCorrectly()
    {
        var number = IncidentNumber.Create("INC-2026-000002");
        var title = IncidentTitle.Create("Tripping Hazard");
        var description = IncidentDescription.Create("Loose cable across walkway");
        var location = IncidentLocation.Create("Hallway A");
        var deptId = Guid.NewGuid();
        var reporterId = Guid.NewGuid();
        var investigatorId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var incident = Incident.Create(
            number, title, description, location,
            IncidentSeverity.Minor, IncidentCategory.NearMiss,
            DateTime.UtcNow, deptId, reporterId, tenantId);

        // Assign
        incident.Assign(investigatorId);
        incident.Status.Should().Be(IncidentStatus.Assigned);
        incident.AssignedToEmployeeId.Should().Be(investigatorId);

        // Start Investigation
        incident.StartInvestigation(investigatorId);
        incident.Status.Should().Be(IncidentStatus.UnderInvestigation);
        incident.InvestigatedByEmployeeId.Should().Be(investigatorId);

        // Add Corrective Action
        var caDesc = CorrectiveActionDescription.Create("Fix cable wiring tray");
        var action = incident.AddCorrectiveAction(caDesc, investigatorId, DateTime.UtcNow.AddDays(7));
        incident.Status.Should().Be(IncidentStatus.WaitingCorrectiveAction);
        incident.CorrectiveActions.Should().HaveCount(1);

        // Complete Corrective Action
        incident.CompleteCorrectiveAction(action.Id, investigatorId);

        // Resolve
        incident.Resolve(InvestigationResult.EquipmentFailure, "Cable tray re-anchored and inspected.");
        incident.Status.Should().Be(IncidentStatus.Resolved);

        // Close
        incident.Close(investigatorId, "Safety audit verified completion.");
        incident.Status.Should().Be(IncidentStatus.Closed);
    }

    [Fact]
    public void Resolve_WithUncompletedCorrectiveActions_ShouldThrowValidationException()
    {
        var incident = Incident.Create(
            IncidentNumber.Create("INC-2026-000003"),
            IncidentTitle.Create("Faulty Switch"),
            IncidentDescription.Create("Sparking light switch"),
            IncidentLocation.Create("Lab 2"),
            IncidentSeverity.Moderate, IncidentCategory.PropertyDamage,
            DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        incident.StartInvestigation(Guid.NewGuid());
        incident.AddCorrectiveAction(CorrectiveActionDescription.Create("Replace switch"), Guid.NewGuid(), DateTime.UtcNow.AddDays(3));

        Action act = () => incident.Resolve(InvestigationResult.EquipmentFailure, "Attempted resolution");
        act.Should().Throw<ValidationException>().Where(ex => ex.Errors.ContainsKey("CorrectiveActions"));
    }
}
