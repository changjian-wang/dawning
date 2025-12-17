using Dawning.Identity.Api;
using Dawning.Identity.Application.Interfaces.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Dawning.Identity.Api.IntegrationTests;

/// <summary>
/// 自定义 WebApplicationFactory，用于配置集成测试环境
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // 使用 Testing 环境，Program.cs 会跳过数据库种子
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // 配置内存分布式缓存（替代 Redis）
            services.AddDistributedMemoryCache();

            // 替换 RequestLoggingService 为空实现（避免连接真实数据库）
            services.RemoveAll<IRequestLoggingService>();
            services.AddSingleton<IRequestLoggingService, NullRequestLoggingService>();
        });
    }
}

/// <summary>
/// 空的请求日志服务实现（用于测试）
/// </summary>
public class NullRequestLoggingService : IRequestLoggingService
{
    public Task LogRequestAsync(RequestLogEntry entry) => Task.CompletedTask;

    public Task<PagedRequestLogs> GetLogsAsync(RequestLogQuery query)
    {
        return Task.FromResult(new PagedRequestLogs
        {
            Items = new List<RequestLogEntry>(),
            TotalCount = 0,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    public Task<RequestStatistics> GetStatisticsAsync(DateTime? startTime = null, DateTime? endTime = null)
    {
        return Task.FromResult(new RequestStatistics
        {
            TotalRequests = 0,
            SuccessRequests = 0,
            ClientErrors = 0,
            ServerErrors = 0,
            AverageResponseTimeMs = 0,
            StartTime = startTime ?? DateTime.UtcNow.AddDays(-1),
            EndTime = endTime ?? DateTime.UtcNow,
            StatusCodeDistribution = new Dictionary<int, long>(),
            TopPaths = new List<PathStatistic>(),
            HourlyRequests = new Dictionary<string, long>()
        });
    }

    public Task<int> CleanupOldLogsAsync(int retentionDays) => Task.FromResult(0);
}
