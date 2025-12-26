using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Dawning.Caching;

/// <summary>
/// 内存缓存服务实现
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly CacheOptions _options;
    private readonly ConcurrentDictionary<string, byte> _keys = new();

    /// <summary>
    /// 初始化内存缓存服务
    /// </summary>
    public MemoryCacheService(IMemoryCache cache, IOptions<CacheOptions> options)
    {
        _cache = cache;
        _options = options.Value;
    }

    /// <inheritdoc />
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        var value = _cache.Get<T>(fullKey);
        return Task.FromResult(value);
    }

    /// <inheritdoc />
    public Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default
    )
    {
        var fullKey = GetFullKey(key);
        var actualExpiration =
            expiration ?? TimeSpan.FromMinutes(_options.DefaultExpirationMinutes);

        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = actualExpiration,
        };

        _cache.Set(fullKey, value, options);
        _keys.TryAdd(fullKey, 0);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<T?> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default
    )
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
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        _cache.Remove(fullKey);
        _keys.TryRemove(fullKey, out _);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        var fullPattern = GetFullKey(pattern);
        var regex = new System.Text.RegularExpressions.Regex(
            "^"
                + System.Text.RegularExpressions.Regex.Escape(fullPattern).Replace("\\*", ".*")
                + "$",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );

        var keysToRemove = _keys.Keys.Where(k => regex.IsMatch(k)).ToList();

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
            _keys.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);
        return Task.FromResult(_cache.TryGetValue(fullKey, out _));
    }

    /// <inheritdoc />
    public async Task RefreshAsync(
        string key,
        TimeSpan expiration,
        CancellationToken cancellationToken = default
    )
    {
        var value = await GetAsync<object>(key, cancellationToken);
        if (value is not null)
        {
            await SetAsync(key, value, expiration, cancellationToken);
        }
    }

    private string GetFullKey(string key)
    {
        return string.IsNullOrEmpty(_options.KeyPrefix) ? key : $"{_options.KeyPrefix}:{key}";
    }
}
