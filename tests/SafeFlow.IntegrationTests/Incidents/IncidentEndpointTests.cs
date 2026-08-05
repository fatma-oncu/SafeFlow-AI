using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SafeFlow.Application.Employees.DTOs;
using SafeFlow.Application.Identity.DTOs;
using SafeFlow.Application.Incidents.DTOs;
using SafeFlow.Domain.Incidents.Enums;
using SafeFlow.IntegrationTests.Infrastructure;
using SafeFlow.SharedKernel.Results;
using Xunit;

namespace SafeFlow.IntegrationTests.Incidents;

[Collection("IntegrationTests")]
public sealed class IncidentEndpointTests
{
    private readonly HttpClient _client;

    public IncidentEndpointTests(SafeFlowWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> GetAuthTokenAsync()
    {
        var loginResp = await _client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "testadmin@safeflow.io",
            password = "TestAdmin@Pass1!"
        });
        var loginData = await loginResp.Content.ReadFromJsonAsync<LoginResponseDto>();
        return loginData!.AccessToken;
    }

    [Fact]
    public async Task GetIncidents_WithoutToken_ShouldReturn401()
    {
        var response = await _client.GetAsync("/api/v1/incidents");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task FullIncidentLifecycle_ShouldSucceed()
    {
        string token = await GetAuthTokenAsync();

        // 1. Create Employee
        var deptId = Guid.NewGuid();
        var empReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/employees")
        {
            Content = JsonContent.Create(new
            {
                firstName = "Incident",
                lastName = "Reporter",
                email = $"reporter_{Guid.NewGuid():N}@safeflow.io",
                departmentId = deptId,
                jobTitle = "Safety Officer",
                employmentType = 1,
                hireDate = DateTime.UtcNow.AddDays(-50),
                tenantId = Guid.NewGuid()
            })
        };
        empReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var empResp = await _client.SendAsync(empReq);
        empResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var empDto = await empResp.Content.ReadFromJsonAsync<EmployeeDto>();
        Guid reporterId = empDto!.Id;

        // 2. Create Incident
        var createReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/incidents")
        {
            Content = JsonContent.Create(new
            {
                title = "Chemical Storage Leak",
                description = "Minor solvent leak identified in Drum #4",
                location = "Chemical Storage Room 3B",
                severity = IncidentSeverity.Moderate,
                category = IncidentCategory.Environmental,
                occurredAt = DateTime.UtcNow.AddHours(-2),
                departmentId = deptId,
                reportedByEmployeeId = reporterId,
                tenantId = empDto.TenantId
            })
        };
        createReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var createResp = await _client.SendAsync(createReq);
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdDto = await createResp.Content.ReadFromJsonAsync<IncidentDto>();
        createdDto.Should().NotBeNull();
        createdDto!.Status.Should().Be("Reported");
        createdDto.IncidentNumber.Should().StartWith("INC-");
        Guid incidentId = createdDto.Id;

        // 3. Assign Incident
        var assignReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/incidents/{incidentId}/assign")
        {
            Content = JsonContent.Create(new { assignedToEmployeeId = reporterId })
        };
        assignReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var assignResp = await _client.SendAsync(assignReq);
        assignResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Start Investigation
        var investReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/incidents/{incidentId}/start-investigation")
        {
            Content = JsonContent.Create(new { investigatorEmployeeId = reporterId })
        };
        investReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var investResp = await _client.SendAsync(investReq);
        investResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // 5. Add Comment
        var commentReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/incidents/{incidentId}/comments")
        {
            Content = JsonContent.Create(new { authorEmployeeId = reporterId, content = "Containment boom applied around Drum #4." })
        };
        commentReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var commentResp = await _client.SendAsync(commentReq);
        commentResp.StatusCode.Should().Be(HttpStatusCode.Created);

        // 6. Add Corrective Action
        var caReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/incidents/{incidentId}/corrective-actions")
        {
            Content = JsonContent.Create(new
            {
                description = "Replace seal ring on Drum #4 and re-inspect storage area",
                assignedToEmployeeId = reporterId,
                dueDate = DateTime.UtcNow.AddDays(2)
            })
        };
        caReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var caResp = await _client.SendAsync(caReq);
        caResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var caDto = await caResp.Content.ReadFromJsonAsync<CorrectiveActionDto>();

        // 7. Complete Corrective Action
        var completeCaReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/incidents/{incidentId}/corrective-actions/{caDto!.Id}/complete")
        {
            Content = JsonContent.Create(new { completedByEmployeeId = reporterId })
        };
        completeCaReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var completeCaResp = await _client.SendAsync(completeCaReq);
        completeCaResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // 8. Resolve Incident
        var resolveReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/incidents/{incidentId}/resolve")
        {
            Content = JsonContent.Create(new
            {
                investigationResult = InvestigationResult.EquipmentFailure,
                resolutionSummary = "Defective seal ring replaced. Area neutralized and cleared."
            })
        };
        resolveReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var resolveResp = await _client.SendAsync(resolveReq);
        resolveResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // 9. Close Incident
        var closeReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/incidents/{incidentId}/close")
        {
            Content = JsonContent.Create(new
            {
                closedByEmployeeId = reporterId,
                closureNotes = "Formally verified by Safety Committee."
            })
        };
        closeReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var closeResp = await _client.SendAsync(closeReq);
        closeResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var closedDto = await closeResp.Content.ReadFromJsonAsync<IncidentDto>();
        closedDto!.Status.Should().Be("Closed");

        // 10. Search Endpoint
        var searchReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/incidents/search?q=Chemical&departmentId={deptId}&status=Closed");
        searchReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var searchResp = await _client.SendAsync(searchReq);
        searchResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var searchResult = await searchResp.Content.ReadFromJsonAsync<PagedResult<IncidentSearchResultDto>>();
        searchResult!.Items.Should().NotBeEmpty();

        // 11. Soft Delete
        var deleteReq = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/incidents/{incidentId}");
        deleteReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var deleteResp = await _client.SendAsync(deleteReq);
        deleteResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify 404
        var getDeletedReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/incidents/{incidentId}");
        getDeletedReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var getDeletedResp = await _client.SendAsync(getDeletedReq);
        getDeletedResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
