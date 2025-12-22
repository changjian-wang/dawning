using System.Net;
using FluentAssertions;

namespace Dawning.Identity.Api.IntegrationTests.Controllers;

/// <summary>
/// 请求日志控制器集成测试
/// 端点: 
///   GET /api/request-logs
///   GET /api/request-logs/statistics
///   GET /api/request-logs/errors
///   GET /api/request-logs/slow
///   DELETE /api/request-logs/cleanup
/// </summary>
public class RequestLogControllerTests : IntegrationTestBase
{
    public RequestLogControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    #region 未认证测试

    [Fact]
    public async Task GetRequestLogs_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/request-logs");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetStatistics_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/request-logs/statistics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetErrors_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/request-logs/errors");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSlowRequests_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/request-logs/slow");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Cleanup_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.DeleteAsync("/api/request-logs/cleanup");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region 端点存在性测试

    [Fact]
    public async Task RequestLogsEndpoint_Exists()
    {
        // Act
        var response = await Client.GetAsync("/api/request-logs");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task StatisticsEndpoint_Exists()
    {
        // Act
        var response = await Client.GetAsync("/api/request-logs/statistics");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ErrorsEndpoint_Exists()
    {
        // Act
        var response = await Client.GetAsync("/api/request-logs/errors");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SlowRequestsEndpoint_Exists()
    {
        // Act
        var response = await Client.GetAsync("/api/request-logs/slow");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CleanupEndpoint_Exists()
    {
        // Act
        var response = await Client.DeleteAsync("/api/request-logs/cleanup");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    #endregion

    #region 查询参数测试

    [Fact]
    public async Task RequestLogs_WithPagination_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/request-logs?page=1&pageSize=20");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RequestLogs_WithMethodFilter_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/request-logs?method=GET");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RequestLogs_WithPathFilter_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/request-logs?path=/api/user");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RequestLogs_WithStatusCodeFilter_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/request-logs?statusCode=200");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RequestLogs_WithTimeRangeFilter_ReturnsUnauthorized()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddDays(-1).ToString("O");
        var endTime = DateTime.UtcNow.ToString("O");

        // Act
        var response = await Client.GetAsync($"/api/request-logs?startTime={startTime}&endTime={endTime}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RequestLogs_WithOnlyErrorsFilter_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/request-logs?onlyErrors=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SlowRequests_WithThreshold_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/request-logs/slow?thresholdMs=500");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Statistics_WithTimeRange_ReturnsUnauthorized()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddDays(-7).ToString("O");
        var endTime = DateTime.UtcNow.ToString("O");

        // Act
        var response = await Client.GetAsync($"/api/request-logs/statistics?startTime={startTime}&endTime={endTime}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Cleanup_WithRetentionDays_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.DeleteAsync("/api/request-logs/cleanup?retentionDays=30");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region 错误响应测试

    [Fact]
    public async Task RequestLogs_WithInvalidPage_ReturnsUnauthorized()
    {
        // Act (页码小于1会被自动修正，但需要先通过认证)
        var response = await Client.GetAsync("/api/request-logs?page=-1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RequestLogs_WithLargePageSize_ReturnsUnauthorized()
    {
        // Act (大于100的pageSize会被限制为100，但需要先通过认证)
        var response = await Client.GetAsync("/api/request-logs?pageSize=1000");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}
