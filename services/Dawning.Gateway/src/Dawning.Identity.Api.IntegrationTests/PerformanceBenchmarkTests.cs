using System.Diagnostics;
using FluentAssertions;
using Xunit.Abstractions;

namespace Dawning.Identity.Api.IntegrationTests;

/// <summary>
/// API 性能基准测试
/// 验证关键 API 端点的响应时间是否符合要求
/// </summary>
public class PerformanceBenchmarkTests : IntegrationTestBase
{
    private readonly ITestOutputHelper _output;

    // 响应时间阈值（毫秒）
    private const int StandardThreshold = 500; // 标准 API 响应时间
    private const int FastThreshold = 200; // 快速 API 响应时间
    private const int SlowThreshold = 1000; // 允许较慢的操作

    public PerformanceBenchmarkTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
        : base(factory)
    {
        _output = output;
    }

    [Fact]
    public async Task HealthCheck_ShouldRespondWithin200ms()
    {
        // Act
        var (response, elapsed) = await MeasureRequestAsync(() => Client.GetAsync("/api/health"));

        // Assert
        _output.WriteLine($"GET /api/health - {elapsed}ms");
        response.IsSuccessStatusCode.Should().BeTrue();
        elapsed.Should().BeLessThan(FastThreshold, "健康检查应在 200ms 内响应");
    }

    [Fact]
    public async Task HealthLive_ShouldRespondWithin200ms()
    {
        // Act
        var (response, elapsed) = await MeasureRequestAsync(() =>
            Client.GetAsync("/api/health/live")
        );

        // Assert
        _output.WriteLine($"GET /api/health/live - {elapsed}ms");
        response.IsSuccessStatusCode.Should().BeTrue();
        elapsed.Should().BeLessThan(FastThreshold, "存活检查应在 200ms 内响应");
    }

    [Fact]
    public async Task UserList_Unauthorized_ShouldRespondWithin500ms()
    {
        // Act
        var (response, elapsed) = await MeasureRequestAsync(() =>
            Client.GetAsync("/api/user?page=1&pageSize=10")
        );

        // Assert
        _output.WriteLine($"GET /api/user - {elapsed}ms (401 Unauthorized)");
        elapsed.Should().BeLessThan(StandardThreshold, "用户列表 API 应在 500ms 内响应");
    }

    [Fact]
    public async Task RoleList_Unauthorized_ShouldRespondWithin500ms()
    {
        // Act
        var (response, elapsed) = await MeasureRequestAsync(() => Client.GetAsync("/api/role"));

        // Assert
        _output.WriteLine($"GET /api/role - {elapsed}ms");
        elapsed.Should().BeLessThan(StandardThreshold, "角色列表 API 应在 500ms 内响应");
    }

    [Fact]
    public async Task PermissionList_Unauthorized_ShouldRespondWithin500ms()
    {
        // Act
        var (response, elapsed) = await MeasureRequestAsync(() =>
            Client.GetAsync("/api/permission")
        );

        // Assert
        _output.WriteLine($"GET /api/permission - {elapsed}ms");
        elapsed.Should().BeLessThan(StandardThreshold, "权限列表 API 应在 500ms 内响应");
    }

    [Fact]
    public async Task AuditLogList_Unauthorized_ShouldRespondWithin500ms()
    {
        // Act
        var (response, elapsed) = await MeasureRequestAsync(() =>
            Client.GetAsync("/api/audit-log?page=1&pageSize=20")
        );

        // Assert
        _output.WriteLine($"GET /api/audit-log - {elapsed}ms");
        elapsed.Should().BeLessThan(StandardThreshold, "审计日志 API 应在 500ms 内响应");
    }

    [Fact]
    public async Task MonitoringLogs_Unauthorized_ShouldRespondWithin500ms()
    {
        // Act
        var (response, elapsed) = await MeasureRequestAsync(() =>
            Client.GetAsync("/api/monitoring/logs?page=1&pageSize=20")
        );

        // Assert
        _output.WriteLine($"GET /api/monitoring/logs - {elapsed}ms");
        elapsed.Should().BeLessThan(StandardThreshold, "监控日志 API 应在 500ms 内响应");
    }

    [Fact]
    public async Task TokenRequest_InvalidCredentials_ShouldRespondWithin1000ms()
    {
        // Arrange
        var content = new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = "test@example.com",
                ["password"] = "wrongpassword",
                ["client_id"] = "dawning-admin",
                ["scope"] = "openid profile",
            }
        );

        // Act
        var (response, elapsed) = await MeasureRequestAsync(() =>
            Client.PostAsync("/connect/token", content)
        );

        // Assert
        _output.WriteLine($"POST /connect/token - {elapsed}ms");
        elapsed.Should().BeLessThan(SlowThreshold, "Token 请求应在 1000ms 内响应");
    }

    [Fact]
    public async Task ConcurrentRequests_ShouldMaintainPerformance()
    {
        // Arrange
        const int concurrentRequests = 10;
        var tasks = new List<Task<(HttpResponseMessage, long)>>();

        // Act - 并发请求
        for (int i = 0; i < concurrentRequests; i++)
        {
            tasks.Add(MeasureRequestAsync(() => Client.GetAsync("/api/health")));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        var times = results.Select(r => r.Item2).ToList();
        var avgTime = times.Average();
        var maxTime = times.Max();

        _output.WriteLine($"并发 {concurrentRequests} 个请求:");
        _output.WriteLine($"  平均响应时间: {avgTime:F1}ms");
        _output.WriteLine($"  最大响应时间: {maxTime}ms");
        _output.WriteLine($"  各请求时间: [{string.Join(", ", times)}]ms");

        avgTime.Should().BeLessThan(StandardThreshold, "并发请求平均响应时间应在 500ms 内");
        maxTime.Should().BeLessThan(SlowThreshold, "并发请求最大响应时间应在 1000ms 内");
    }

    [Fact]
    public async Task MultipleSequentialRequests_ShouldBeConsistent()
    {
        // Arrange
        const int requestCount = 5;
        var times = new List<long>();

        // Act - 连续请求
        for (int i = 0; i < requestCount; i++)
        {
            var (_, elapsed) = await MeasureRequestAsync(() => Client.GetAsync("/api/health"));
            times.Add(elapsed);
        }

        // Assert
        var avgTime = times.Average();
        var stdDev = Math.Sqrt(times.Select(t => Math.Pow(t - avgTime, 2)).Average());

        _output.WriteLine($"连续 {requestCount} 个请求:");
        _output.WriteLine($"  平均响应时间: {avgTime:F1}ms");
        _output.WriteLine($"  标准差: {stdDev:F1}ms");
        _output.WriteLine($"  各请求时间: [{string.Join(", ", times)}]ms");

        avgTime.Should().BeLessThan(FastThreshold, "连续请求平均响应时间应在 200ms 内");
        stdDev.Should().BeLessThan(100, "响应时间波动不应太大");
    }

    /// <summary>
    /// 测量请求响应时间
    /// </summary>
    private async Task<(HttpResponseMessage Response, long ElapsedMs)> MeasureRequestAsync(
        Func<Task<HttpResponseMessage>> requestFunc
    )
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await requestFunc();
        stopwatch.Stop();
        return (response, stopwatch.ElapsedMilliseconds);
    }
}
