using System.Net;
using FluentAssertions;

namespace Dawning.Identity.Api.IntegrationTests.Controllers;

/// <summary>
/// 健康检查控制器集成测试
/// </summary>
public class HealthControllerTests : IntegrationTestBase
{
    public HealthControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

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
        // 可能是 200 (Healthy) 或 503 (Unhealthy，如果数据库不可用)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("checks");
    }

    [Fact]
    public async Task Get_HealthReadyEndpoint_ReturnsReadinessStatus()
    {
        // Act
        var response = await Client.GetAsync("/api/health/ready");

        // Assert
        // 取决于数据库是否可用
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task Get_HealthLiveEndpoint_ReturnsLivenessStatus()
    {
        // Act
        var response = await Client.GetAsync("/api/health/live");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Live");
    }

    [Fact]
    public async Task Get_HealthChecksEndpoint_ReturnsOk()
    {
        // Act - 内置 ASP.NET Core 健康检查端点
        var response = await Client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
