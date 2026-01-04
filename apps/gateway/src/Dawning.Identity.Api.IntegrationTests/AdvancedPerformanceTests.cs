using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Xunit.Abstractions;

namespace Dawning.Identity.Api.IntegrationTests;

/// <summary>
/// Advanced performance and stress tests
/// Tests system behavior under high load
/// </summary>
public class AdvancedPerformanceTests : IntegrationTestBase
{
    private readonly ITestOutputHelper _output;

    // Performance thresholds (milliseconds)
    private const int FastApiThreshold = 200;
    private const int StandardApiThreshold = 500;
    private const int HighLoadThreshold = 2000;

    public AdvancedPerformanceTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
        : base(factory)
    {
        _output = output;
    }

    [Fact]
    public async Task HighConcurrency_HealthCheck_ShouldHandle50Requests()
    {
        // Arrange
        const int concurrentRequests = 50;
        var tasks = new List<Task<(HttpResponseMessage, long)>>();

        // Act - High concurrency requests
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < concurrentRequests; i++)
        {
            tasks.Add(MeasureRequestAsync(() => Client.GetAsync("/api/health")));
        }

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        var times = results.Select(r => r.Item2).ToList();
        var successCount = results.Count(r => r.Item1.IsSuccessStatusCode);
        var avgTime = times.Average();
        var p95Time = times.OrderBy(t => t).ElementAt((int)(times.Count * 0.95));
        var p99Time = times.OrderBy(t => t).ElementAt((int)(times.Count * 0.99));

        _output.WriteLine($"High concurrency test ({concurrentRequests} requests):");
        _output.WriteLine(
            $"  Success rate: {successCount}/{concurrentRequests} ({successCount * 100.0 / concurrentRequests:F1}%)"
        );
        _output.WriteLine($"  Total time: {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"  Average response time: {avgTime:F1}ms");
        _output.WriteLine($"  P95 response time: {p95Time}ms");
        _output.WriteLine($"  P99 response time: {p99Time}ms");
        _output.WriteLine($"  Min/Max: {times.Min()}ms / {times.Max()}ms");

        successCount.Should().Be(concurrentRequests, "All requests should succeed");
        p95Time.Should().BeLessThan(HighLoadThreshold, "P95 response time should be within 2000ms");
    }

    [Fact]
    public async Task BurstTraffic_ShouldHandleSpikes()
    {
        // Arrange - Simulate traffic spikes
        const int burstSize = 20;
        const int burstCount = 3;
        var allResults = new List<(HttpResponseMessage, long)>();

        // Act - Multiple burst requests
        for (int burst = 0; burst < burstCount; burst++)
        {
            var tasks = Enumerable
                .Range(0, burstSize)
                .Select(_ => MeasureRequestAsync(() => Client.GetAsync("/api/health")))
                .ToList();

            var results = await Task.WhenAll(tasks);
            allResults.AddRange(results);

            // Short delay before next burst
            await Task.Delay(100);
        }

        // Assert
        var times = allResults.Select(r => r.Item2).ToList();
        var successCount = allResults.Count(r => r.Item1.IsSuccessStatusCode);
        var avgTime = times.Average();

        _output.WriteLine($"Burst traffic test ({burstCount} bursts, {burstSize} requests each):");
        _output.WriteLine($"  Total requests: {allResults.Count}");
        _output.WriteLine($"  Success rate: {successCount * 100.0 / allResults.Count:F1}%");
        _output.WriteLine($"  Average response time: {avgTime:F1}ms");

        successCount.Should().Be(allResults.Count, "All burst requests should succeed");
        avgTime.Should().BeLessThan(StandardApiThreshold, "Average response time should be within 500ms");
    }

    [Fact]
    public async Task MixedEndpoints_ConcurrentRequests_ShouldAllSucceed()
    {
        // Arrange - Mixed requests to different endpoints (excluding /api/health/ready that requires database)
        var endpoints = new[]
        {
            "/api/health",
            "/api/health/live",
            "/api/user",
            "/api/role",
            "/api/permission",
        };

        var tasks = new List<Task<(HttpResponseMessage, long)>>();

        // Act - Concurrent requests to multiple different endpoints
        var stopwatch = Stopwatch.StartNew();
        foreach (var endpoint in endpoints)
        {
            for (int i = 0; i < 5; i++)
            {
                var ep = endpoint; // Capture for closure
                tasks.Add(MeasureRequestAsync(() => Client.GetAsync(ep)));
            }
        }

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        var times = results.Select(r => r.Item2).ToList();
        // Count valid responses (success or requires authentication are normal)
        var validResponses = results.Count(r =>
            r.Item1.IsSuccessStatusCode || r.Item1.StatusCode == HttpStatusCode.Unauthorized
        );

        _output.WriteLine($"Mixed endpoints concurrency test:");
        _output.WriteLine($"  Endpoint count: {endpoints.Length}");
        _output.WriteLine($"  Total requests: {tasks.Count}");
        _output.WriteLine($"  Valid responses: {validResponses}");
        _output.WriteLine($"  Total time: {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"  Average response time: {times.Average():F1}ms");

        validResponses.Should().Be(tasks.Count, "All requests should return valid responses");
    }

    [Fact]
    public async Task ResponseTimeStability_Over10Requests()
    {
        // Arrange
        const int requestCount = 10;
        var times = new List<long>();

        // Act - Continuous requests and record time
        for (int i = 0; i < requestCount; i++)
        {
            var (response, elapsed) = await MeasureRequestAsync(() =>
                Client.GetAsync("/api/health")
            );
            times.Add(elapsed);
            response.EnsureSuccessStatusCode();
        }

        // Assert - Calculate statistics
        var avgTime = times.Average();
        var stdDev = Math.Sqrt(times.Select(t => Math.Pow(t - avgTime, 2)).Average());
        var coefficientOfVariation = stdDev / avgTime * 100;

        _output.WriteLine($"Response time stability test ({requestCount} requests):");
        _output.WriteLine($"  Response times: [{string.Join(", ", times)}]ms");
        _output.WriteLine($"  Average: {avgTime:F1}ms");
        _output.WriteLine($"  Standard deviation: {stdDev:F1}ms");
        _output.WriteLine($"  Coefficient of variation: {coefficientOfVariation:F1}%");

        // Coefficient of variation should be less than 100% (indicates relative stability)
        coefficientOfVariation.Should().BeLessThan(100, "Response time coefficient of variation should be less than 100%");
    }

    [Fact]
    public async Task FirstRequest_VsSubsequentRequests_WarmupEffect()
    {
        // Arrange
        const int warmupRequests = 3;
        const int measuredRequests = 5;
        var warmupTimes = new List<long>();
        var measuredTimes = new List<long>();

        // Act - Warmup requests
        for (int i = 0; i < warmupRequests; i++)
        {
            var (_, elapsed) = await MeasureRequestAsync(() => Client.GetAsync("/api/health"));
            warmupTimes.Add(elapsed);
        }

        // Wait a moment for system to stabilize
        await Task.Delay(100);

        // Act - Measurement requests
        for (int i = 0; i < measuredRequests; i++)
        {
            var (_, elapsed) = await MeasureRequestAsync(() => Client.GetAsync("/api/health"));
            measuredTimes.Add(elapsed);
        }

        // Assert
        var warmupAvg = warmupTimes.Average();
        var measuredAvg = measuredTimes.Average();

        _output.WriteLine($"Warmup effect test:");
        _output.WriteLine(
            $"  Warmup requests ({warmupRequests}): [{string.Join(", ", warmupTimes)}]ms, average {warmupAvg:F1}ms"
        );
        _output.WriteLine(
            $"  Measurement requests ({measuredRequests}): [{string.Join(", ", measuredTimes)}]ms, average {measuredAvg:F1}ms"
        );

        // Average time during measurement phase should be stable
        measuredAvg.Should().BeLessThan(FastApiThreshold, "Requests after warmup should respond within 200ms");
    }

    [Fact]
    public async Task LargePayload_TokenRequest_ShouldHandle()
    {
        // Arrange - Send a larger request
        var content = new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = "test@example.com",
                ["password"] = "TestPassword123!",
                ["client_id"] = "dawning-admin",
                ["scope"] = "openid profile email offline_access api roles",
            }
        );

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await Client.PostAsync("/connect/token", content);
        stopwatch.Stop();

        // Assert
        _output.WriteLine($"Token request test:");
        _output.WriteLine($"  Response time: {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"  Status code: {response.StatusCode}");

        stopwatch
            .ElapsedMilliseconds.Should()
            .BeLessThan(HighLoadThreshold, "Token request should respond within 2000ms");
    }

    [Fact]
    public async Task Pagination_DifferentPageSizes_Performance()
    {
        // Arrange
        var pageSizes = new[] { 10, 20, 50, 100 };
        var results = new Dictionary<int, long>();

        // Act - Test performance of different pagination sizes
        foreach (var pageSize in pageSizes)
        {
            var (_, elapsed) = await MeasureRequestAsync(() =>
                Client.GetAsync($"/api/user?page=1&pageSize={pageSize}")
            );
            results[pageSize] = elapsed;
        }

        // Assert
        _output.WriteLine($"Pagination performance test:");
        foreach (var (pageSize, time) in results)
        {
            _output.WriteLine($"  pageSize={pageSize}: {time}ms");
        }

        // All pagination requests should respond within reasonable time
        results.Values.Should().AllSatisfy(t => t.Should().BeLessThan(StandardApiThreshold));
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
