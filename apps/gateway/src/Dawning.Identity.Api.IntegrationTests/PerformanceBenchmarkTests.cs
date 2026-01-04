using System.Diagnostics;
using FluentAssertions;
using Xunit.Abstractions;

namespace Dawning.Identity.Api.IntegrationTests;

/// <summary>
/// API performance benchmark tests
/// Verify that key API endpoint response times meet requirements
/// </summary>
public class PerformanceBenchmarkTests : IntegrationTestBase
{
    private readonly ITestOutputHelper _output;

    // Response time thresholds (milliseconds)
    private const int StandardThreshold = 500; // Standard API response time
    private const int FastThreshold = 200; // Fast API response time
    private const int SlowThreshold = 1000; // Allowed for slower operations

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
        elapsed.Should().BeLessThan(FastThreshold, "Health check should respond within 200ms");
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
        elapsed.Should().BeLessThan(FastThreshold, "Liveness check should respond within 200ms");
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
        elapsed.Should().BeLessThan(StandardThreshold, "User list API should respond within 500ms");
    }

    [Fact]
    public async Task RoleList_Unauthorized_ShouldRespondWithin500ms()
    {
        // Act
        var (response, elapsed) = await MeasureRequestAsync(() => Client.GetAsync("/api/role"));

        // Assert
        _output.WriteLine($"GET /api/role - {elapsed}ms");
        elapsed.Should().BeLessThan(StandardThreshold, "Role list API should respond within 500ms");
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
        elapsed.Should().BeLessThan(StandardThreshold, "Permission list API should respond within 500ms");
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
        elapsed.Should().BeLessThan(StandardThreshold, "Audit log API should respond within 500ms");
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
        elapsed.Should().BeLessThan(StandardThreshold, "Monitoring log API should respond within 500ms");
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
        elapsed.Should().BeLessThan(SlowThreshold, "Token request should respond within 1000ms");
    }

    [Fact]
    public async Task ConcurrentRequests_ShouldMaintainPerformance()
    {
        // Arrange
        const int concurrentRequests = 10;
        var tasks = new List<Task<(HttpResponseMessage, long)>>();

        // Act - Concurrent requests
        for (int i = 0; i < concurrentRequests; i++)
        {
            tasks.Add(MeasureRequestAsync(() => Client.GetAsync("/api/health")));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        var times = results.Select(r => r.Item2).ToList();
        var avgTime = times.Average();
        var maxTime = times.Max();

        _output.WriteLine($"Concurrent {concurrentRequests} requests:");
        _output.WriteLine($"  Average response time: {avgTime:F1}ms");
        _output.WriteLine($"  Maximum response time: {maxTime}ms");
        _output.WriteLine($"  Individual request times: [{string.Join(", ", times)}]ms");

        avgTime.Should().BeLessThan(StandardThreshold, "Concurrent requests average response time should be within 500ms");
        maxTime.Should().BeLessThan(SlowThreshold, "Concurrent requests maximum response time should be within 1000ms");
    }

    [Fact]
    public async Task MultipleSequentialRequests_ShouldBeConsistent()
    {
        // Arrange
        const int requestCount = 5;
        var times = new List<long>();

        // Act - Sequential requests
        for (int i = 0; i < requestCount; i++)
        {
            var (_, elapsed) = await MeasureRequestAsync(() => Client.GetAsync("/api/health"));
            times.Add(elapsed);
        }

        // Assert
        var avgTime = times.Average();
        var stdDev = Math.Sqrt(times.Select(t => Math.Pow(t - avgTime, 2)).Average());

        _output.WriteLine($"Sequential {requestCount} requests:");
        _output.WriteLine($"  Average response time: {avgTime:F1}ms");
        _output.WriteLine($"  Standard deviation: {stdDev:F1}ms");
        _output.WriteLine($"  Individual request times: [{string.Join(", ", times)}]ms");

        avgTime.Should().BeLessThan(FastThreshold, "Sequential requests average response time should be within 200ms");
        stdDev.Should().BeLessThan(100, "Response time variance should not be too large");
    }

    /// <summary>
    /// Measure request response time
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
