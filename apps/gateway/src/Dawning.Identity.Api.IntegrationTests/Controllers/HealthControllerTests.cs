using System.Net;
using FluentAssertions;

namespace Dawning.Identity.Api.IntegrationTests.Controllers;

/// <summary>
/// Health check controller integration tests
/// </summary>
public class HealthControllerTests : IntegrationTestBase
{
    public HealthControllerTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task Get_HealthEndpoint_ReturnsHealthyStatus()
    {
        // Act
        var response = await Client.GetAsync("/api/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
        content.Should().Contain("timestamp");
        content.Should().Contain("uptime");
    }

    [Fact]
    public async Task Get_HealthDetailedEndpoint_ReturnsDetailedStatus()
    {
        // Act
        var response = await Client.GetAsync("/api/health/detailed");

        // Assert
        // May be 200 (Healthy) or 503 (Unhealthy, if database is unavailable)
        response
            .StatusCode.Should()
            .BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("checks");
    }

    [Fact]
    public async Task Get_HealthReadyEndpoint_ReturnsReadinessStatus()
    {
        // Act
        var response = await Client.GetAsync("/api/health/ready");

        // Assert
        // Depends on whether database is available
        response
            .StatusCode.Should()
            .BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task Get_HealthLiveEndpoint_ReturnsLivenessStatus()
    {
        // Act
        var response = await Client.GetAsync("/api/health/live");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Alive");
    }

    [Fact]
    public async Task Get_HealthChecksEndpoint_ReturnsOk()
    {
        // Act - Built-in ASP.NET Core health check endpoint
        var response = await Client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
