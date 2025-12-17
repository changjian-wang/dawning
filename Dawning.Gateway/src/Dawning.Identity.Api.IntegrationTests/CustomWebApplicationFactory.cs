using Dawning.Identity.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dawning.Identity.Api.IntegrationTests;

/// <summary>
/// 自定义 WebApplicationFactory，用于配置集成测试环境
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // 移除数据库种子服务，避免测试时自动执行
            var descriptorsToRemove = services
                .Where(d => d.ServiceType.FullName?.Contains("DatabaseSeeder") == true)
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // 配置内存分布式缓存（替代 Redis）
            services.AddDistributedMemoryCache();

            // 可以在这里添加测试专用的服务配置
            // 例如：使用内存数据库、Mock 服务等
        });
    }
}
