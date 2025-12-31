using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;

namespace Dawning.Caching;

/// <summary>
/// Cache service extension methods
/// </summary>
public static class CachingServiceCollectionExtensions
{
    /// <summary>
    /// Add Dawning caching service
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configure">Configuration delegate</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddDawningCaching(
        this IServiceCollection services,
        Action<CacheOptions>? configure = null
    )
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
    /// Add memory cache service
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configure">Configuration delegate</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddDawningMemoryCache(
        this IServiceCollection services,
        Action<CacheOptions>? configure = null
    )
    {
        return services.AddDawningCaching(options =>
        {
            options.Provider = CacheProvider.Memory;
            configure?.Invoke(options);
        });
    }

    /// <summary>
    /// Add Redis cache service
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="connectionString">Redis connection string</param>
    /// <param name="configure">Configuration delegate</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddDawningRedisCache(
        this IServiceCollection services,
        string connectionString,
        Action<CacheOptions>? configure = null
    )
    {
        return services.AddDawningCaching(options =>
        {
            options.Provider = CacheProvider.Redis;
            options.Redis.ConnectionString = connectionString;
            configure?.Invoke(options);
        });
    }
}
