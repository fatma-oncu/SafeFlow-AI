using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SafeFlow.Application.Employees.DTOs;
using SafeFlow.Application.Identity.DTOs;
using SafeFlow.Application.RiskAssessments.DTOs;
using SafeFlow.Domain.RiskAssessments.Enums;
using SafeFlow.IntegrationTests.Infrastructure;
using SafeFlow.SharedKernel.Results;
using Xunit;

namespace SafeFlow.IntegrationTests.RiskAssessments;

public sealed class RiskAssessmentEndpointTests : IClassFixture<SafeFlowWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly SafeFlowWebApplicationFactory _factory;

    public RiskAssessmentEndpointTests(SafeFlowWebApplicationFactory factory)
    {
        _factory = factory;
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
    public async Task CreateRiskAssessment_WithoutToken_ShouldReturn401()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/risk-assessments")
        {
            Content = JsonContent.Create(new
            {
                title = "Unauthorized Test",
                description = "Should fail",
                departmentId = Guid.NewGuid(),
                createdByEmployeeId = Guid.NewGuid(),
                responsibleEmployeeId = Guid.NewGuid(),
                tenantId = Guid.NewGuid()
            })
        };

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task FullRiskAssessmentLifecycle_ShouldSucceed()
    {
        string token = await GetAuthTokenAsync();

        // 1. Create Employee for Responsible & CreatedBy
        var deptId = Guid.NewGuid();
        var empReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/employees")
        {
            Content = JsonContent.Create(new
            {
                firstName = "Risk",
                lastName = "Officer",
                email = $"riskofficer_{Guid.NewGuid():N}@safeflow.io",
                departmentId = deptId,
                jobTitle = "Safety Officer",
                employmentType = 1,
                hireDate = DateTime.UtcNow.AddDays(-100),
                tenantId = Guid.NewGuid()
            })
        };
        empReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var empResp = await _client.SendAsync(empReq);
        empResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var empDto = await empResp.Content.ReadFromJsonAsync<EmployeeDto>();
        Guid responsibleEmpId = empDto!.Id;

        // 2. Create Risk Assessment
        var createReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/risk-assessments")
        {
            Content = JsonContent.Create(new
            {
                title = "High Voltage Maintenance",
                description = "Electrical substation safety review",
                departmentId = deptId,
                createdByEmployeeId = responsibleEmpId,
                responsibleEmployeeId = responsibleEmpId,
                tenantId = empDto.TenantId,
                nextReviewDate = DateTime.UtcNow.AddYears(1)
            })
        };
        createReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var createResp = await _client.SendAsync(createReq);
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdDto = await createResp.Content.ReadFromJsonAsync<RiskAssessmentDto>();
        createdDto.Should().NotBeNull();
        createdDto!.Status.Should().Be("Draft");
        createdDto.AssessmentNumber.Should().StartWith("RA-");
        createdDto.History.Should().NotBeEmpty();
        createdDto.History.Should().ContainSingle(h => h.Action == "Created");

        Guid assessmentId = createdDto.Id;

        // 3. Update Risk Assessment details
        var updateReq = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/risk-assessments/{assessmentId}")
        {
            Content = JsonContent.Create(new
            {
                title = "Updated High Voltage Substation Review",
                description = "Updated description with enhanced controls",
                departmentId = deptId,
                responsibleEmployeeId = responsibleEmpId,
                nextReviewDate = DateTime.UtcNow.AddMonths(6),
                rowVersion = createdDto.RowVersion
            })
        };
        updateReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var updateResp = await _client.SendAsync(updateReq);
        updateResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedDto = await updateResp.Content.ReadFromJsonAsync<RiskAssessmentDto>();
        updatedDto!.Title.Should().Be("Updated High Voltage Substation Review");

        // 4. Test Concurrency Conflict (PUT with stale RowVersion)
        var conflictReq = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/risk-assessments/{assessmentId}")
        {
            Content = JsonContent.Create(new
            {
                title = "Conflicting Update",
                description = "Should fail with 409",
                departmentId = deptId,
                responsibleEmployeeId = responsibleEmpId,
                nextReviewDate = DateTime.UtcNow.AddMonths(6),
                rowVersion = createdDto.RowVersion // Stale version
            })
        };
        conflictReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var conflictResp = await _client.SendAsync(conflictReq);
        conflictResp.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // 5. Add Hazard
        var addHazardReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/risk-assessments/{assessmentId}/hazards")
        {
            Content = JsonContent.Create(new
            {
                description = "Electrocution hazard during maintenance",
                initialLikelihood = 4,
                initialSeverity = 4,
                residualLikelihood = 2,
                residualSeverity = 2
            })
        };
        addHazardReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var addHazardResp = await _client.SendAsync(addHazardReq);
        addHazardResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var hazardDto = await addHazardResp.Content.ReadFromJsonAsync<RiskHazardDto>();
        hazardDto.Should().NotBeNull();
        hazardDto!.InitialScore.Should().Be(16);
        hazardDto.InitialRiskLevel.Should().Be("Critical");
        hazardDto.ResidualScore.Should().Be(4);
        hazardDto.ResidualRiskLevel.Should().Be("Low");

        // 6. Add Control Measure
        var addControlReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/risk-assessments/{assessmentId}/hazards/{hazardDto.Id}/controls")
        {
            Content = JsonContent.Create(new
            {
                description = "Lockout/Tagout PPE gear and safety supervisor presence",
                type = ControlMeasureType.Engineering,
                isImplemented = true
            })
        };
        addControlReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var addControlResp = await _client.SendAsync(addControlReq);
        addControlResp.StatusCode.Should().Be(HttpStatusCode.Created);

        // 7. Submit for Review
        var submitReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/risk-assessments/{assessmentId}/submit?submittedByEmployeeId={responsibleEmpId}");
        submitReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var submitResp = await _client.SendAsync(submitReq);
        submitResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var submittedDto = await submitResp.Content.ReadFromJsonAsync<RiskAssessmentDto>();
        submittedDto!.Status.Should().Be("InReview");
        submittedDto.Approvals.Should().HaveCount(1);
        submittedDto.Approvals[0].Decision.Should().Be("Submitted");

        // 8. Approve Assessment with Comment
        var approveReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/risk-assessments/{assessmentId}/approve")
        {
            Content = JsonContent.Create(new
            {
                approverEmployeeId = responsibleEmpId,
                comment = "Compliance verified and approved."
            })
        };
        approveReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var approveResp = await _client.SendAsync(approveReq);
        approveResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var approvedDto = await approveResp.Content.ReadFromJsonAsync<RiskAssessmentDto>();
        approvedDto!.Status.Should().Be("Approved");
        approvedDto.ApprovedByEmployeeId.Should().Be(responsibleEmpId);
        approvedDto.Approvals.Should().HaveCount(2);

        // 9. Get by ID & Verify History Audit Log
        var getReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/risk-assessments/{assessmentId}");
        getReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var getResp = await _client.SendAsync(getReq);
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var getDto = await getResp.Content.ReadFromJsonAsync<RiskAssessmentDto>();
        getDto!.History.Should().HaveCountGreaterThanOrEqualTo(5);

        // 10. Search & Filter
        var searchReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/risk-assessments/search?q=Voltage&departmentId={deptId}&status=Approved");
        searchReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var searchResp = await _client.SendAsync(searchReq);
        searchResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var searchResult = await searchResp.Content.ReadFromJsonAsync<PagedResult<RiskAssessmentSearchResultDto>>();
        searchResult!.Items.Should().NotBeEmpty();

        // 11. Soft Delete Assessment
        var deleteReq = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/risk-assessments/{assessmentId}");
        deleteReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var deleteResp = await _client.SendAsync(deleteReq);
        deleteResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify 404 after soft delete
        var getDeletedReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/risk-assessments/{assessmentId}");
        getDeletedReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var getDeletedResp = await _client.SendAsync(getDeletedReq);
        getDeletedResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
