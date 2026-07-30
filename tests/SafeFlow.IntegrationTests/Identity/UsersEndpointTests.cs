using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SafeFlow.Application.Identity.DTOs;
using SafeFlow.IntegrationTests.Infrastructure;
using Xunit;

namespace SafeFlow.IntegrationTests.Identity;

/// <summary>
/// End-to-end integration tests for user profile endpoints.
/// </summary>
[Collection("IntegrationTests")]
public sealed class UsersEndpointTests
{
    private readonly SafeFlowWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public UsersEndpointTests(SafeFlowWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCurrentUser_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        var response = await _client.GetAsync("/api/v1/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_WithAuthenticatedUser_Returns200OK()
    {
        // 1. Register and login a user to get an access token
        var email = $"user_me_{Guid.NewGuid():N}@example.com";
        var password = "Password1!";

        var regResp = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            firstName = "Test",
            lastName = "User",
            tenantId = Guid.NewGuid()
        });

        regResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var loginResp = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        var loginContent = await loginResp.Content.ReadAsStringAsync();
        loginResp.StatusCode.Should().Be(HttpStatusCode.OK, because: $"Login failed: {loginContent}");

        var loginData = await loginResp.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginData.Should().NotBeNull();
        loginData!.AccessToken.Should().NotBeNullOrWhiteSpace();

        // 2. Query /api/v1/users/me with Bearer token
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/users/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginData.AccessToken);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var userDto = await response.Content.ReadFromJsonAsync<UserDto>();
        userDto.Should().NotBeNull();
        userDto!.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetUserById_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        var response = await _client.GetAsync($"/api/v1/users/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
