using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SafeFlow.Application.Employees.DTOs;
using SafeFlow.Application.Identity.DTOs;
using SafeFlow.Domain.Employees.Enums;
using SafeFlow.IntegrationTests.Infrastructure;
using SafeFlow.SharedKernel.Results;
using Xunit;

namespace SafeFlow.IntegrationTests.Employees;

[Collection("IntegrationTests")]
public sealed class EmployeeEndpointTests
{
    private readonly SafeFlowWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public EmployeeEndpointTests(SafeFlowWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> GetAuthTokenAsync()
    {
        string email = $"admin_emp_{Guid.NewGuid():N}@example.com";
        string password = "Password1!";

        await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            firstName = "Admin",
            lastName = "User",
            tenantId = Guid.NewGuid()
        });

        var loginResp = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        var loginData = await loginResp.Content.ReadFromJsonAsync<LoginResponseDto>();
        return loginData!.AccessToken;
    }

    [Fact]
    public async Task GetEmployees_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        var response = await _client.GetAsync("/api/v1/employees");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateEmployee_WithAuthenticatedAdmin_Returns201Created()
    {
        var token = await GetAuthTokenAsync();
        var departmentId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/employees");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new
        {
            firstName = "Alice",
            lastName = "Smith",
            email = $"alice_{Guid.NewGuid():N}@example.com",
            phoneNumber = "+1234567890",
            departmentId,
            jobTitle = "Software Engineer",
            employmentType = EmploymentType.FullTime,
            hireDate = DateTime.UtcNow,
            tenantId
        });

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var employee = await response.Content.ReadFromJsonAsync<EmployeeDto>();
        employee.Should().NotBeNull();
        employee!.FirstName.Should().Be("Alice");
        employee.LastName.Should().Be("Smith");
        employee.EmployeeNumber.Should().StartWith("EMP-");
    }

    [Fact]
    public async Task CreateEmployee_WithDuplicateEmail_Returns409Conflict()
    {
        var token = await GetAuthTokenAsync();
        string email = $"dup_{Guid.NewGuid():N}@example.com";
        var deptId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var req1 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/employees");
        req1.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req1.Content = JsonContent.Create(new
        {
            firstName = "First",
            lastName = "User",
            email,
            departmentId = deptId,
            jobTitle = "Dev",
            employmentType = EmploymentType.FullTime,
            hireDate = DateTime.UtcNow,
            tenantId
        });
        var resp1 = await _client.SendAsync(req1);
        resp1.StatusCode.Should().Be(HttpStatusCode.Created);

        var req2 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/employees");
        req2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req2.Content = JsonContent.Create(new
        {
            firstName = "Second",
            lastName = "User",
            email,
            departmentId = deptId,
            jobTitle = "Dev",
            employmentType = EmploymentType.FullTime,
            hireDate = DateTime.UtcNow,
            tenantId
        });
        var resp2 = await _client.SendAsync(req2);
        resp2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetEmployeeById_Returns200OK()
    {
        var token = await GetAuthTokenAsync();

        var createReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/employees");
        createReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        createReq.Content = JsonContent.Create(new
        {
            firstName = "Bob",
            lastName = "Jones",
            email = $"bob_{Guid.NewGuid():N}@example.com",
            departmentId = Guid.NewGuid(),
            jobTitle = "QA Lead",
            employmentType = EmploymentType.FullTime,
            hireDate = DateTime.UtcNow,
            tenantId = Guid.NewGuid()
        });
        var createResp = await _client.SendAsync(createReq);
        var createdEmp = await createResp.Content.ReadFromJsonAsync<EmployeeDto>();

        var getReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/employees/{createdEmp!.Id}");
        getReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var getResp = await _client.SendAsync(getReq);

        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetchedEmp = await getResp.Content.ReadFromJsonAsync<EmployeeDto>();
        fetchedEmp.Should().NotBeNull();
        fetchedEmp!.Id.Should().Be(createdEmp.Id);
    }

    [Fact]
    public async Task UpdateEmployee_And_Transfer_Returns200OK()
    {
        var token = await GetAuthTokenAsync();

        var createReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/employees");
        createReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        createReq.Content = JsonContent.Create(new
        {
            firstName = "Charlie",
            lastName = "Brown",
            email = $"charlie_{Guid.NewGuid():N}@example.com",
            departmentId = Guid.NewGuid(),
            jobTitle = "Junior Dev",
            employmentType = EmploymentType.FullTime,
            hireDate = DateTime.UtcNow,
            tenantId = Guid.NewGuid()
        });
        var createResp = await _client.SendAsync(createReq);
        var createdEmp = await createResp.Content.ReadFromJsonAsync<EmployeeDto>();

        // Update
        var updateReq = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/employees/{createdEmp!.Id}");
        updateReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        updateReq.Content = JsonContent.Create(new
        {
            firstName = "Charles",
            lastName = "Brown",
            email = createdEmp.Email,
            phoneNumber = "+9876543210",
            jobTitle = "Mid Software Engineer",
            rowVersion = createdEmp.RowVersion
        });
        var updateResp = await _client.SendAsync(updateReq);
        updateResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Transfer
        var newDept = Guid.NewGuid();
        var transferReq = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/employees/{createdEmp.Id}/transfer");
        transferReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        transferReq.Content = JsonContent.Create(new
        {
            newDepartmentId = newDept,
            rowVersion = createdEmp.RowVersion
        });
        var transferResp = await _client.SendAsync(transferReq);
        transferResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SoftDeleteEmployee_And_GetById_Returns404NotFound()
    {
        var token = await GetAuthTokenAsync();

        var createReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/employees");
        createReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        createReq.Content = JsonContent.Create(new
        {
            firstName = "David",
            lastName = "Miller",
            email = $"david_{Guid.NewGuid():N}@example.com",
            departmentId = Guid.NewGuid(),
            jobTitle = "DevOps Engineer",
            employmentType = EmploymentType.Contractor,
            hireDate = DateTime.UtcNow,
            tenantId = Guid.NewGuid()
        });
        var createResp = await _client.SendAsync(createReq);
        var createdEmp = await createResp.Content.ReadFromJsonAsync<EmployeeDto>();

        // Soft Delete
        var delReq = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/employees/{createdEmp!.Id}");
        delReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var delResp = await _client.SendAsync(delReq);
        delResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // GetById after soft delete -> 404
        var getReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/employees/{createdEmp.Id}");
        getReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var getResp = await _client.SendAsync(getReq);
        getResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SearchEmployees_ReturnsPagedResults()
    {
        var token = await GetAuthTokenAsync();
        string uniqueKey = Guid.NewGuid().ToString("N")[..8];

        var createReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/employees");
        createReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        createReq.Content = JsonContent.Create(new
        {
            firstName = $"Search_{uniqueKey}",
            lastName = "Target",
            email = $"search_{uniqueKey}@example.com",
            departmentId = Guid.NewGuid(),
            jobTitle = "Search Analyst",
            employmentType = EmploymentType.FullTime,
            hireDate = DateTime.UtcNow,
            tenantId = Guid.NewGuid()
        });
        await _client.SendAsync(createReq);

        var searchReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/employees/search?q={uniqueKey}&page=1&pageSize=10");
        searchReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var searchResp = await _client.SendAsync(searchReq);

        searchResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedData = await searchResp.Content.ReadFromJsonAsync<PagedResult<EmployeeSearchResultDto>>();
        pagedData.Should().NotBeNull();
        pagedData!.Items.Should().ContainSingle(e => e.FirstName.Contains(uniqueKey));
    }
}
