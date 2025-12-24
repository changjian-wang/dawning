using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Dawning.Caching;

/// <summary>
/// Redis 分布式缓存服务实现
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly CacheOptions _options;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// 初始化 Redis 缓存服务
    /// </summary>
    public RedisCacheService(IDistributedCache cache, IOptions<CacheOptions> options)
    {
        _cache = cache;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        var data = await _cache.GetStringAsync(fullKey, cancellationToken);

        if (string.IsNullOrEmpty(data))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(data, JsonOptions);
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        var actualExpiration = expiration ?? TimeSpan.FromMinutes(_options.DefaultExpirationMinutes);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = actualExpiration
        };

        var data = JsonSerializer.Serialize(value, JsonOptions);
        await _cache.SetStringAsync(fullKey, data, options, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var existing = await GetAsync<T>(key, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var value = await factory();
        await SetAsync(key, value, expiration, cancellationToken);
        return value;
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        await _cache.RemoveAsync(fullKey, cancellationToken);
    }

    /// <inheritdoc />
    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        // Redis 的模式删除需要使用 StackExchange.Redis 直接操作
        // IDistributedCache 接口不支持此功能
        // 如需此功能，请直接注入 IConnectionMultiplexer
        throw new NotSupportedException("Pattern-based removal requires direct Redis connection. Use IConnectionMultiplexer instead.");
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        var data = await _cache.GetAsync(fullKey, cancellationToken);
        return data is not null;
    }

    /// <inheritdoc />
    public async Task RefreshAsync(string key, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        await _cache.RefreshAsync(fullKey, cancellationToken);
    }

    private string GetFullKey(string key)
    {
        return string.IsNullOrEmpty(_options.KeyPrefix) ? key : $"{_options.KeyPrefix}:{key}";
    }
}
