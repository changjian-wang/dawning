using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace Dawning.Caching.Tests;

public class MemoryCacheServiceTests
{
    private readonly MemoryCacheService _cacheService;

    public MemoryCacheServiceTests()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var options = Options.Create(
            new CacheOptions { DefaultExpirationMinutes = 30, KeyPrefix = "test" }
        );
        _cacheService = new MemoryCacheService(memoryCache, options);
    }

    [Fact]
    public async Task SetAsync_And_GetAsync_ShouldWork()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";

        // Act
        await _cacheService.SetAsync(key, value);
        var result = await _cacheService.GetAsync<string>(key);

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public async Task GetAsync_NonExistentKey_ShouldReturnDefault()
    {
        // Act
        var result = await _cacheService.GetAsync<string>("non-existent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetOrSetAsync_WhenKeyExists_ShouldReturnCachedValue()
    {
        // Arrange
        var key = "existing-key";
        var cachedValue = "cached-value";
        await _cacheService.SetAsync(key, cachedValue);

        // Act
        var result = await _cacheService.GetOrSetAsync(key, () => Task.FromResult("new-value"));

        // Assert
        result.Should().Be(cachedValue);
    }

    [Fact]
    public async Task GetOrSetAsync_WhenKeyNotExists_ShouldCallFactory()
    {
        // Arrange
        var key = "new-key";
        var newValue = "new-value";
        var factoryCalled = false;

        // Act
        var result = await _cacheService.GetOrSetAsync(
            key,
            () =>
            {
                factoryCalled = true;
                return Task.FromResult(newValue);
            }
        );

        // Assert
        result.Should().Be(newValue);
        factoryCalled.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveKey()
    {
        // Arrange
        var key = "key-to-remove";
        await _cacheService.SetAsync(key, "value");

        // Act
        await _cacheService.RemoveAsync(key);
        var result = await _cacheService.GetAsync<string>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_WhenKeyExists_ShouldReturnTrue()
    {
        // Arrange
        var key = "exists-key";
        await _cacheService.SetAsync(key, "value");

        // Act
        var result = await _cacheService.ExistsAsync(key);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenKeyNotExists_ShouldReturnFalse()
    {
        // Act
        var result = await _cacheService.ExistsAsync("not-exists");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveByPatternAsync_ShouldRemoveMatchingKeys()
    {
        // Arrange
        await _cacheService.SetAsync("user:1", "value1");
        await _cacheService.SetAsync("user:2", "value2");
        await _cacheService.SetAsync("order:1", "order-value");

        // Act
        await _cacheService.RemoveByPatternAsync("user:*");

        // Assert
        (await _cacheService.ExistsAsync("user:1"))
            .Should()
            .BeFalse();
        (await _cacheService.ExistsAsync("user:2")).Should().BeFalse();
        (await _cacheService.ExistsAsync("order:1")).Should().BeTrue();
    }

    [Fact]
    public async Task SetAsync_WithCustomExpiration_ShouldWork()
    {
        // Arrange
        var key = "expiring-key";
        var value = "expiring-value";

        // Act
        await _cacheService.SetAsync(key, value, TimeSpan.FromSeconds(1));
        var beforeExpire = await _cacheService.GetAsync<string>(key);

        // Assert
        beforeExpire.Should().Be(value);
    }

    [Fact]
    public async Task SetAsync_WithComplexObject_ShouldWork()
    {
        // Arrange
        var key = "complex-key";
        var value = new TestObject
        {
            Id = 1,
            Name = "Test",
            CreatedAt = DateTime.UtcNow,
        };

        // Act
        await _cacheService.SetAsync(key, value);
        var result = await _cacheService.GetAsync<TestObject>(key);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(value.Id);
        result.Name.Should().Be(value.Name);
    }

    private class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
