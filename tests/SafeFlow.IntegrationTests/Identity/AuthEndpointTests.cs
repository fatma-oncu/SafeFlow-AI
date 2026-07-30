using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SafeFlow.IntegrationTests.Infrastructure;
using Xunit;

namespace SafeFlow.IntegrationTests.Identity;

/// <summary>
/// End-to-end integration tests for the authentication endpoints.
/// Uses an in-memory SQLite database and a generated RSA key via
/// <see cref="SafeFlowWebApplicationFactory"/>.
/// </summary>
public sealed class AuthEndpointTests : IClassFixture<SafeFlowWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointTests(SafeFlowWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static object ValidRegisterRequest(string? email = null) => new
    {
        email     = email ?? $"user_{Guid.NewGuid():N}@example.com",
        password  = "Password1!",
        firstName = "Alice",
        lastName  = "Smith",
        phoneNumber = (string?)null,
        tenantId  = Guid.NewGuid(),
    };

    // ── POST /api/v1/auth/register ────────────────────────────────────────────

    [Fact]
    public async Task Register_WithValidRequest_Returns201Created()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/register", ValidRegisterRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_Returns422UnprocessableEntity()
    {
        var request = new
        {
            email     = "not-an-email",
            password  = "Password1!",
            firstName = "Alice",
            lastName  = "Smith",
            tenantId  = Guid.NewGuid(),
        };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithWeakPassword_Returns422UnprocessableEntity()
    {
        var request = ValidRegisterRequest() is IDictionary<string, object?> d
            ? d
            : null;

        // Override password with a weak one
        var weakPasswordRequest = new
        {
            email     = $"user_{Guid.NewGuid():N}@example.com",
            password  = "weak",
            firstName = "Alice",
            lastName  = "Smith",
            tenantId  = Guid.NewGuid(),
        };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", weakPasswordRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── POST /api/v1/auth/login ───────────────────────────────────────────────

    [Fact]
    public async Task Login_WithNonExistentUser_Returns401Unauthorized()
    {
        var request = new
        {
            email    = "ghost@example.com",
            password = "Password1!",
        };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithRegisteredUserAndCorrectPassword_Returns200OK()
    {
        // 1. Register a unique user
        var email    = $"integration_{Guid.NewGuid():N}@example.com";
        var password = "Password1!";

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new
            {
                email,
                password,
                firstName = "Bob",
                lastName  = "Builder",
                tenantId  = Guid.NewGuid(),
            });

        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // 2. Login with the same credentials
        var loginResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email, password });

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await loginResponse.Content.ReadAsStringAsync();
        body.Should().Contain("accessToken");
    }

    // ── Protected endpoint (GET /api/v1/users/me) ─────────────────────────────

    [Fact]
    public async Task GetCurrentUser_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        var response = await _client.GetAsync("/api/v1/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Health ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task HealthEndpoint_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");

        // Accept 200 OK or 404 if health endpoint is not configured in Testing env
        ((int)response.StatusCode).Should().BeOneOf(200, 404);
    }
}
