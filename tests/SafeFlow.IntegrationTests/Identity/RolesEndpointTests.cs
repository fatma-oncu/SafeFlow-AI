using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SafeFlow.Application.Identity.DTOs;
using SafeFlow.IntegrationTests.Infrastructure;
using Xunit;

namespace SafeFlow.IntegrationTests.Identity;

/// <summary>
/// End-to-end integration tests for role endpoints.
/// </summary>
[Collection("IntegrationTests")]
public sealed class RolesEndpointTests
{
    private readonly HttpClient _client;

    public RolesEndpointTests(SafeFlowWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetRoles_WithoutAuthorizationHeader_Returns401Unauthorized()
    {
        var response = await _client.GetAsync("/api/v1/roles");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
