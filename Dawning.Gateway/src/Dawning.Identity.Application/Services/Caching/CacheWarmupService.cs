using System;
using System.Threading;
using System.Threading.Tasks;
using Dawning.Identity.Application.Interfaces.Caching;
using Dawning.Identity.Domain.Interfaces.UoW;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dawning.Identity.Application.Services.Caching
{
    /// <summary>
    /// 缓存预热服务
    /// 在应用启动时预加载热点数据到缓存
    /// </summary>
    public class CacheWarmupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CacheWarmupService> _logger;

        public CacheWarmupService(
            IServiceProvider serviceProvider,
            ILogger<CacheWarmupService> logger
        )
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 延迟启动，等待应用完全初始化
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            _logger.LogInformation("Starting cache warmup...");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var cacheService = scope.ServiceProvider.GetService<ICacheService>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                if (cacheService == null)
                {
                    _logger.LogWarning("Cache service not available, skipping warmup");
                    return;
                }

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                // 顺序预热缓存（MySQL 不支持在同一连接上并行执行多个查询）
                await WarmupSystemConfigAsync(cacheService, unitOfWork, stoppingToken);
                await WarmupRolesAsync(cacheService, unitOfWork, stoppingToken);
                await WarmupPermissionsAsync(cacheService, unitOfWork, stoppingToken);
                await WarmupRateLimitPoliciesAsync(cacheService, unitOfWork, stoppingToken);
                await WarmupIpRulesAsync(cacheService, unitOfWork, stoppingToken);

                stopwatch.Stop();
                _logger.LogInformation(
                    "Cache warmup completed in {ElapsedMs}ms",
                    stopwatch.ElapsedMilliseconds
                );
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Cache warmup cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cache warmup");
            }
        }

        /// <summary>
        /// 预热系统配置
        /// </summary>
        private async Task WarmupSystemConfigAsync(
            ICacheService cacheService,
            IUnitOfWork unitOfWork,
            CancellationToken cancellationToken
        )
        {
            try
            {
                // 获取所有系统配置（分页获取前1000条）
                var result = await unitOfWork.SystemConfig.GetPagedListAsync(
                    new Dawning.Identity.Domain.Models.Administration.SystemConfigModel(),
                    1,
                    1000
                );
                var configs = result.Items;
                var count = 0;

                foreach (var config in configs)
                {
                    if (config.Name != null && config.Key != null)
                    {
                        var key = CacheKeys.SystemConfig(config.Name, config.Key);
                        await cacheService.SetAsync(
                            key,
                            config.Value,
                            CacheEntryOptions.Long,
                            cancellationToken
                        );
                        count++;
                    }
                }

                _logger.LogDebug("Warmed up {Count} system configs", count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to warmup system configs");
            }
        }

        /// <summary>
        /// 预热角色数据
        /// </summary>
        private async Task WarmupRolesAsync(
            ICacheService cacheService,
            IUnitOfWork unitOfWork,
            CancellationToken cancellationToken
        )
        {
            try
            {
                var roles = await unitOfWork.Role.GetAllAsync();
                var roleList = roles.ToList();

                // 缓存所有角色列表
                await cacheService.SetAsync(
                    CacheKeys.AllRoles,
                    roleList,
                    CacheEntryOptions.Medium,
                    cancellationToken
                );

                // 缓存单个角色
                foreach (var role in roleList)
                {
                    await cacheService.SetAsync(
                        CacheKeys.Role(role.Id),
                        role,
                        CacheEntryOptions.Medium,
                        cancellationToken
                    );

                    await cacheService.SetAsync(
                        CacheKeys.RoleByName(role.Name),
                        role,
                        CacheEntryOptions.Medium,
                        cancellationToken
                    );
                }

                _logger.LogDebug("Warmed up {Count} roles", roleList.Count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to warmup roles");
            }
        }

        /// <summary>
        /// 预热权限数据
        /// </summary>
        private async Task WarmupPermissionsAsync(
            ICacheService cacheService,
            IUnitOfWork unitOfWork,
            CancellationToken cancellationToken
        )
        {
            try
            {
                var permissions = await unitOfWork.Permission.GetAllAsync();
                var permissionList = permissions.ToList();

                // 缓存所有权限列表
                await cacheService.SetAsync(
                    CacheKeys.AllPermissions,
                    permissionList,
                    CacheEntryOptions.Medium,
                    cancellationToken
                );

                _logger.LogDebug("Warmed up {Count} permissions", permissionList.Count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to warmup permissions");
            }
        }

        /// <summary>
        /// 预热限流策略
        /// </summary>
        private async Task WarmupRateLimitPoliciesAsync(
            ICacheService cacheService,
            IUnitOfWork unitOfWork,
            CancellationToken cancellationToken
        )
        {
            try
            {
                var policies = await unitOfWork.RateLimitPolicy.GetAllAsync();
                var policyList = policies.ToList();

                await cacheService.SetAsync(
                    "dawning:ratelimit:policies",
                    policyList,
                    CacheEntryOptions.Long,
                    cancellationToken
                );

                _logger.LogDebug("Warmed up {Count} rate limit policies", policyList.Count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to warmup rate limit policies");
            }
        }

        /// <summary>
        /// 预热 IP 规则
        /// </summary>
        private async Task WarmupIpRulesAsync(
            ICacheService cacheService,
            IUnitOfWork unitOfWork,
            CancellationToken cancellationToken
        )
        {
            try
            {
                // 获取黑名单和白名单规则
                var blacklistRules = await unitOfWork.IpAccessRule.GetActiveRulesByTypeAsync(
                    "blacklist"
                );
                var whitelistRules = await unitOfWork.IpAccessRule.GetActiveRulesByTypeAsync(
                    "whitelist"
                );
                var allRules = blacklistRules.Concat(whitelistRules).ToList();

                await cacheService.SetAsync(
                    CacheKeys.IpRules,
                    allRules,
                    CacheEntryOptions.Long,
                    cancellationToken
                );

                _logger.LogDebug("Warmed up {Count} IP access rules", allRules.Count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to warmup IP rules");
            }
        }
    }
}
