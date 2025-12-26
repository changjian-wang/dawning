using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dawning.Caching.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDawningMemoryCache_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDawningMemoryCache(options =>
        {
            options.DefaultExpirationMinutes = 60;
            options.KeyPrefix = "test";
        });

        var provider = services.BuildServiceProvider();
        var cacheService = provider.GetService<ICacheService>();

        // Assert
        cacheService.Should().NotBeNull();
        cacheService.Should().BeOfType<MemoryCacheService>();
    }

    [Fact]
    public void AddDawningCaching_WithMemoryProvider_ShouldRegisterMemoryCache()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDawningCaching(options =>
        {
            options.Provider = CacheProvider.Memory;
        });

        var provider = services.BuildServiceProvider();
        var cacheService = provider.GetService<ICacheService>();

        // Assert
        cacheService.Should().BeOfType<MemoryCacheService>();
    }

    [Fact]
    public void AddDawningCaching_WithRedisProvider_ShouldRegisterRedisCache()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDawningCaching(options =>
        {
            options.Provider = CacheProvider.Redis;
            options.Redis.ConnectionString = "localhost:6379";
        });

        var provider = services.BuildServiceProvider();
        var cacheService = provider.GetService<ICacheService>();

        // Assert
        cacheService.Should().BeOfType<RedisCacheService>();
    }

    [Fact]
    public void AddDawningRedisCache_ShouldConfigureConnectionString()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "redis.example.com:6379";

        // Act
        services.AddDawningRedisCache(
            connectionString,
            options =>
            {
                options.KeyPrefix = "myapp";
            }
        );

        var provider = services.BuildServiceProvider();
        var cacheService = provider.GetService<ICacheService>();

        // Assert
        cacheService.Should().BeOfType<RedisCacheService>();
    }
}
