using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dawning.Identity.Application.Interfaces.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Dawning.Identity.Application.Services.Caching
{
    /// <summary>
    /// 基于 IDistributedCache 的缓存服务实现
    /// 支持 Redis 和内存缓存的自动切换
    /// </summary>
    public class DistributedCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<DistributedCacheService> _logger;

        // 用于防止缓存击穿的锁
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
        };

        public DistributedCacheService(
            IDistributedCache cache,
            ILogger<DistributedCacheService> logger
        )
        {
            _cache = cache;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<T?> GetOrSetAsync<T>(
            string key,
            Func<CancellationToken, Task<T?>> factory,
            CacheEntryOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            // 先尝试从缓存获取
            var cached = await GetAsync<T>(key, cancellationToken);
            if (cached != null)
            {
                return cached;
            }

            // 获取或创建该 key 的锁，防止缓存击穿
            var lockObj = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

            await lockObj.WaitAsync(cancellationToken);
            try
            {
                // 双重检查：可能在等待锁期间其他线程已设置缓存
                cached = await GetAsync<T>(key, cancellationToken);
                if (cached != null)
                {
                    return cached;
                }

                // 调用工厂方法获取数据
                var value = await factory(cancellationToken);

                // 缓存结果
                if (value != null)
                {
                    await SetAsync(
                        key,
                        value,
                        options ?? CacheEntryOptions.Default,
                        cancellationToken
                    );
                }

                return value;
            }
            finally
            {
                lockObj.Release();

                // 清理不再使用的锁对象
                if (lockObj.CurrentCount == 1)
                {
                    _locks.TryRemove(key, out _);
                }
            }
        }

        /// <inheritdoc />
        public async Task<T?> GetOrSetWithNullProtectionAsync<T>(
            string key,
            Func<CancellationToken, Task<T?>> factory,
            CacheEntryOptions? options = null,
            TimeSpan? nullValueTtl = null,
            CancellationToken cancellationToken = default
        )
            where T : class
        {
            // 先尝试从缓存获取
            var cachedString = await _cache.GetStringAsync(key, cancellationToken);

            if (!string.IsNullOrEmpty(cachedString))
            {
                // 检查是否是空值标记
                if (cachedString == CacheKeys.NullMarker)
                {
                    _logger.LogDebug("Cache hit with null marker for key: {Key}", key);
                    return null;
                }

                try
                {
                    return JsonSerializer.Deserialize<T>(cachedString, _jsonOptions);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize cache value for key: {Key}", key);
                }
            }

            // 获取锁
            var lockObj = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

            await lockObj.WaitAsync(cancellationToken);
            try
            {
                // 双重检查
                cachedString = await _cache.GetStringAsync(key, cancellationToken);
                if (!string.IsNullOrEmpty(cachedString))
                {
                    if (cachedString == CacheKeys.NullMarker)
                    {
                        return null;
                    }

                    try
                    {
                        return JsonSerializer.Deserialize<T>(cachedString, _jsonOptions);
                    }
                    catch
                    {
                        // 忽略反序列化错误
                    }
                }

                // 调用工厂方法
                var value = await factory(cancellationToken);

                if (value != null)
                {
                    await SetAsync(
                        key,
                        value,
                        options ?? CacheEntryOptions.Default,
                        cancellationToken
                    );
                }
                else
                {
                    // 缓存空值以防止缓存穿透
                    var nullTtl = nullValueTtl ?? TimeSpan.FromMinutes(1);
                    await _cache.SetStringAsync(
                        key,
                        CacheKeys.NullMarker,
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = nullTtl,
                        },
                        cancellationToken
                    );
                    _logger.LogDebug(
                        "Cached null marker for key: {Key} with TTL: {Ttl}",
                        key,
                        nullTtl
                    );
                }

                return value;
            }
            finally
            {
                lockObj.Release();
                if (lockObj.CurrentCount == 1)
                {
                    _locks.TryRemove(key, out _);
                }
            }
        }

        /// <inheritdoc />
        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var cachedString = await _cache.GetStringAsync(key, cancellationToken);

                if (string.IsNullOrEmpty(cachedString) || cachedString == CacheKeys.NullMarker)
                {
                    return default;
                }

                return JsonSerializer.Deserialize<T>(cachedString, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get cache for key: {Key}", key);
                return default;
            }
        }

        /// <inheritdoc />
        public async Task SetAsync<T>(
            string key,
            T value,
            CacheEntryOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            if (value == null)
            {
                return;
            }

            try
            {
                var serialized = JsonSerializer.Serialize(value, _jsonOptions);
                var distributedOptions = ToDistributedCacheOptions(
                    options ?? CacheEntryOptions.Default
                );

                await _cache.SetStringAsync(key, serialized, distributedOptions, cancellationToken);

                _logger.LogDebug("Cache set for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to set cache for key: {Key}", key);
            }
        }

        /// <inheritdoc />
        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                await _cache.RemoveAsync(key, cancellationToken);
                _logger.LogDebug("Cache removed for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to remove cache for key: {Key}", key);
            }
        }

        /// <inheritdoc />
        public Task RemoveByPrefixAsync(
            string prefix,
            CancellationToken cancellationToken = default
        )
        {
            // 注意：IDistributedCache 不原生支持按前缀删除
            // 如果使用 Redis，可以通过 StackExchange.Redis 的 SCAN + DEL 实现
            // 这里记录警告，实际生产环境应考虑使用 Redis 直接连接
            _logger.LogWarning(
                "RemoveByPrefixAsync is not fully supported with IDistributedCache. "
                    + "Consider using direct Redis connection for prefix deletion. Prefix: {Prefix}",
                prefix
            );

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task<bool> ExistsAsync(
            string key,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var value = await _cache.GetAsync(key, cancellationToken);
                return value != null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to check cache existence for key: {Key}", key);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task RefreshAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                await _cache.RefreshAsync(key, cancellationToken);
                _logger.LogDebug("Cache refreshed for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to refresh cache for key: {Key}", key);
            }
        }

        private static DistributedCacheEntryOptions ToDistributedCacheOptions(
            CacheEntryOptions options
        )
        {
            return new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = options.AbsoluteExpiration,
                AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = options.SlidingExpiration,
            };
        }
    }
}
