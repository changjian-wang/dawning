using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Xunit.Abstractions;

namespace Dawning.Identity.Api.IntegrationTests;

/// <summary>
/// 高级性能和压力测试
/// 测试系统在高负载下的表现
/// </summary>
public class AdvancedPerformanceTests : IntegrationTestBase
{
    private readonly ITestOutputHelper _output;
    
    // 性能阈值（毫秒）
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

        // Act - 高并发请求
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

        _output.WriteLine($"高并发测试 ({concurrentRequests} 请求):");
        _output.WriteLine($"  成功率: {successCount}/{concurrentRequests} ({successCount * 100.0 / concurrentRequests:F1}%)");
        _output.WriteLine($"  总耗时: {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"  平均响应时间: {avgTime:F1}ms");
        _output.WriteLine($"  P95 响应时间: {p95Time}ms");
        _output.WriteLine($"  P99 响应时间: {p99Time}ms");
        _output.WriteLine($"  最小/最大: {times.Min()}ms / {times.Max()}ms");

        successCount.Should().Be(concurrentRequests, "所有请求应该成功");
        p95Time.Should().BeLessThan(HighLoadThreshold, "P95 响应时间应在 2000ms 内");
    }

    [Fact]
    public async Task BurstTraffic_ShouldHandleSpikes()
    {
        // Arrange - 模拟流量突增
        const int burstSize = 20;
        const int burstCount = 3;
        var allResults = new List<(HttpResponseMessage, long)>();

        // Act - 多次突发请求
        for (int burst = 0; burst < burstCount; burst++)
        {
            var tasks = Enumerable.Range(0, burstSize)
                .Select(_ => MeasureRequestAsync(() => Client.GetAsync("/api/health")))
                .ToList();

            var results = await Task.WhenAll(tasks);
            allResults.AddRange(results);

            // 短暂间隔后再次突发
            await Task.Delay(100);
        }

        // Assert
        var times = allResults.Select(r => r.Item2).ToList();
        var successCount = allResults.Count(r => r.Item1.IsSuccessStatusCode);
        var avgTime = times.Average();

        _output.WriteLine($"突发流量测试 ({burstCount} 次突发, 每次 {burstSize} 请求):");
        _output.WriteLine($"  总请求数: {allResults.Count}");
        _output.WriteLine($"  成功率: {successCount * 100.0 / allResults.Count:F1}%");
        _output.WriteLine($"  平均响应时间: {avgTime:F1}ms");

        successCount.Should().Be(allResults.Count, "所有突发请求应该成功");
        avgTime.Should().BeLessThan(StandardApiThreshold, "平均响应时间应在 500ms 内");
    }

    [Fact]
    public async Task MixedEndpoints_ConcurrentRequests_ShouldAllSucceed()
    {
        // Arrange - 混合请求不同的端点（排除需要数据库的 /api/health/ready）
        var endpoints = new[]
        {
            "/api/health",
            "/api/health/live",
            "/api/user",
            "/api/role",
            "/api/permission",
        };

        var tasks = new List<Task<(HttpResponseMessage, long)>>();

        // Act - 并发请求多个不同端点
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
        // 统计有效响应（成功或需要认证都是正常的）
        var validResponses = results.Count(r => 
            r.Item1.IsSuccessStatusCode || 
            r.Item1.StatusCode == HttpStatusCode.Unauthorized);

        _output.WriteLine($"混合端点并发测试:");
        _output.WriteLine($"  端点数量: {endpoints.Length}");
        _output.WriteLine($"  总请求数: {tasks.Count}");
        _output.WriteLine($"  有效响应: {validResponses}");
        _output.WriteLine($"  总耗时: {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"  平均响应时间: {times.Average():F1}ms");

        validResponses.Should().Be(tasks.Count, "所有请求应该返回有效响应");
    }

    [Fact]
    public async Task ResponseTimeStability_Over10Requests()
    {
        // Arrange
        const int requestCount = 10;
        var times = new List<long>();

        // Act - 连续请求并记录时间
        for (int i = 0; i < requestCount; i++)
        {
            var (response, elapsed) = await MeasureRequestAsync(() => Client.GetAsync("/api/health"));
            times.Add(elapsed);
            response.EnsureSuccessStatusCode();
        }

        // Assert - 计算统计指标
        var avgTime = times.Average();
        var stdDev = Math.Sqrt(times.Select(t => Math.Pow(t - avgTime, 2)).Average());
        var coefficientOfVariation = stdDev / avgTime * 100;

        _output.WriteLine($"响应时间稳定性测试 ({requestCount} 请求):");
        _output.WriteLine($"  响应时间: [{string.Join(", ", times)}]ms");
        _output.WriteLine($"  平均值: {avgTime:F1}ms");
        _output.WriteLine($"  标准差: {stdDev:F1}ms");
        _output.WriteLine($"  变异系数: {coefficientOfVariation:F1}%");

        // 变异系数应小于 100%（表示相对稳定）
        coefficientOfVariation.Should().BeLessThan(100, "响应时间变异系数应小于 100%");
    }

    [Fact]
    public async Task FirstRequest_VsSubsequentRequests_WarmupEffect()
    {
        // Arrange
        const int warmupRequests = 3;
        const int measuredRequests = 5;
        var warmupTimes = new List<long>();
        var measuredTimes = new List<long>();

        // Act - 预热请求
        for (int i = 0; i < warmupRequests; i++)
        {
            var (_, elapsed) = await MeasureRequestAsync(() => Client.GetAsync("/api/health"));
            warmupTimes.Add(elapsed);
        }

        // 等待一下让系统稳定
        await Task.Delay(100);

        // Act - 测量请求
        for (int i = 0; i < measuredRequests; i++)
        {
            var (_, elapsed) = await MeasureRequestAsync(() => Client.GetAsync("/api/health"));
            measuredTimes.Add(elapsed);
        }

        // Assert
        var warmupAvg = warmupTimes.Average();
        var measuredAvg = measuredTimes.Average();

        _output.WriteLine($"预热效应测试:");
        _output.WriteLine($"  预热请求 ({warmupRequests}): [{string.Join(", ", warmupTimes)}]ms, 平均 {warmupAvg:F1}ms");
        _output.WriteLine($"  测量请求 ({measuredRequests}): [{string.Join(", ", measuredTimes)}]ms, 平均 {measuredAvg:F1}ms");

        // 测量阶段的平均时间应该稳定
        measuredAvg.Should().BeLessThan(FastApiThreshold, "预热后请求应在 200ms 内响应");
    }

    [Fact]
    public async Task LargePayload_TokenRequest_ShouldHandle()
    {
        // Arrange - 发送较大的请求
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["username"] = "test@example.com",
            ["password"] = "TestPassword123!",
            ["client_id"] = "dawning-admin",
            ["scope"] = "openid profile email offline_access api roles"
        });

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await Client.PostAsync("/connect/token", content);
        stopwatch.Stop();

        // Assert
        _output.WriteLine($"Token 请求测试:");
        _output.WriteLine($"  响应时间: {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"  状态码: {response.StatusCode}");

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(HighLoadThreshold, "Token 请求应在 2000ms 内响应");
    }

    [Fact]
    public async Task Pagination_DifferentPageSizes_Performance()
    {
        // Arrange
        var pageSizes = new[] { 10, 20, 50, 100 };
        var results = new Dictionary<int, long>();

        // Act - 测试不同分页大小的性能
        foreach (var pageSize in pageSizes)
        {
            var (_, elapsed) = await MeasureRequestAsync(() => 
                Client.GetAsync($"/api/user?page=1&pageSize={pageSize}"));
            results[pageSize] = elapsed;
        }

        // Assert
        _output.WriteLine($"分页性能测试:");
        foreach (var (pageSize, time) in results)
        {
            _output.WriteLine($"  pageSize={pageSize}: {time}ms");
        }

        // 所有分页请求应该在合理时间内响应
        results.Values.Should().AllSatisfy(t => t.Should().BeLessThan(StandardApiThreshold));
    }

    /// <summary>
    /// 测量请求响应时间
    /// </summary>
    private async Task<(HttpResponseMessage Response, long ElapsedMs)> MeasureRequestAsync(
        Func<Task<HttpResponseMessage>> requestFunc)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await requestFunc();
        stopwatch.Stop();
        return (response, stopwatch.ElapsedMilliseconds);
    }
}
