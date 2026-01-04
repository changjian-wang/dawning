using Dawning.Identity.Api;
using Dawning.Identity.Application.Interfaces.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Dawning.Identity.Api.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory for configuring integration test environment
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Use Testing environment, Program.cs will skip database seeding
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Configure in-memory distributed cache (replaces Redis)
            services.AddDistributedMemoryCache();

            // Replace RequestLoggingService with null implementation (to avoid connecting to real database)
            services.RemoveAll<IRequestLoggingService>();
            services.AddSingleton<IRequestLoggingService, NullRequestLoggingService>();
        });
    }
}

/// <summary>
/// Null request logging service implementation (for testing)
/// </summary>
public class NullRequestLoggingService : IRequestLoggingService
{
    public Task LogRequestAsync(RequestLogEntry entry) => Task.CompletedTask;

    public Task<PagedRequestLogs> GetLogsAsync(RequestLogQuery query)
    {
        return Task.FromResult(
            new PagedRequestLogs
            {
                Items = new List<RequestLogEntry>(),
                TotalCount = 0,
                Page = query.Page,
                PageSize = query.PageSize,
            }
        );
    }

    public Task<RequestStatistics> GetStatisticsAsync(
        DateTime? startTime = null,
        DateTime? endTime = null
    )
    {
        return Task.FromResult(
            new RequestStatistics
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
                HourlyRequests = new Dictionary<string, long>(),
            }
        );
    }

    public Task<int> CleanupOldLogsAsync(int retentionDays) => Task.FromResult(0);
}
