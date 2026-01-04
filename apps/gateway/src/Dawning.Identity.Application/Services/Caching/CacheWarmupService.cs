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
    /// Cache warmup service.
    /// Pre-loads hot data into cache when the application starts.
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
            // Delay startup, wait for application to fully initialize
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

                // Warm up cache sequentially (MySQL does not support parallel queries on the same connection)
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
        /// Warm up system configuration cache
        /// </summary>
        private async Task WarmupSystemConfigAsync(
            ICacheService cacheService,
            IUnitOfWork unitOfWork,
            CancellationToken cancellationToken
        )
        {
            try
            {
                // Get all system configs (paginated, first 1000 records)
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
        /// Warm up roles cache
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

                // Cache all roles list
                await cacheService.SetAsync(
                    CacheKeys.AllRoles,
                    roleList,
                    CacheEntryOptions.Medium,
                    cancellationToken
                );

                // Cache individual roles
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
        /// Warm up permissions cache
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

                // Cache all permissions list
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
        /// Warm up rate limit policies cache
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
        /// Warm up IP rules cache
        /// </summary>
        private async Task WarmupIpRulesAsync(
            ICacheService cacheService,
            IUnitOfWork unitOfWork,
            CancellationToken cancellationToken
        )
        {
            try
            {
                // Get blacklist and whitelist rules
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
