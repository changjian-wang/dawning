using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;

namespace Dawning.Caching;

/// <summary>
/// 缓存服务扩展方法
/// </summary>
public static class CachingServiceCollectionExtensions
{
    /// <summary>
    /// 添加 Dawning 缓存服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configure">配置委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddDawningCaching(
        this IServiceCollection services,
        Action<CacheOptions>? configure = null)
    {
        var options = new CacheOptions();
        configure?.Invoke(options);

        services.Configure<CacheOptions>(opt =>
        {
            opt.Provider = options.Provider;
            opt.DefaultExpirationMinutes = options.DefaultExpirationMinutes;
            opt.KeyPrefix = options.KeyPrefix;
            opt.Redis = options.Redis;
        });

        switch (options.Provider)
        {
            case CacheProvider.Redis:
                services.AddStackExchangeRedisCache(redisOptions =>
                {
                    redisOptions.Configuration = options.Redis.ConnectionString;
                    redisOptions.InstanceName = options.Redis.InstanceName;
                });
                services.AddSingleton<ICacheService, RedisCacheService>();
                break;

            case CacheProvider.Memory:
            default:
                services.AddMemoryCache();
                services.AddSingleton<ICacheService, MemoryCacheService>();
                break;
        }

        return services;
    }

    /// <summary>
    /// 添加内存缓存服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configure">配置委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddDawningMemoryCache(
        this IServiceCollection services,
        Action<CacheOptions>? configure = null)
    {
        return services.AddDawningCaching(options =>
        {
            options.Provider = CacheProvider.Memory;
            configure?.Invoke(options);
        });
    }

    /// <summary>
    /// 添加 Redis 缓存服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="connectionString">Redis 连接字符串</param>
    /// <param name="configure">配置委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddDawningRedisCache(
        this IServiceCollection services,
        string connectionString,
        Action<CacheOptions>? configure = null)
    {
        return services.AddDawningCaching(options =>
        {
            options.Provider = CacheProvider.Redis;
            options.Redis.ConnectionString = connectionString;
            configure?.Invoke(options);
        });
    }
}
