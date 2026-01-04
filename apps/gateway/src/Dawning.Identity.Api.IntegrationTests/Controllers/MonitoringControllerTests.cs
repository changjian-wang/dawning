using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Dawning.Identity.Api.IntegrationTests.Controllers;

/// <summary>
/// Monitoring controller integration tests
/// Monitoring controller endpoints: GET /api/monitoring/logs, GET /api/monitoring/statistics
/// </summary>
public class MonitoringControllerTests : IntegrationTestBase
{
    public MonitoringControllerTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    #region Unauthenticated tests

    [Fact]
    public async Task GetLogs_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/monitoring/logs");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetStatistics_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/monitoring/statistics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Endpoint existence tests

    [Fact]
    public async Task LogsEndpoint_Exists()
    {
        // Act
        var response = await Client.GetAsync("/api/monitoring/logs");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task StatisticsEndpoint_Exists()
    {
        // Act
        var response = await Client.GetAsync("/api/monitoring/statistics");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Logs_WithFilters_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/monitoring/logs?method=GET&page=1&pageSize=20");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}
