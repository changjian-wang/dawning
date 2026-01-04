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
    /// Cache service implementation based on IDistributedCache.
    /// Supports automatic switching between Redis and in-memory cache.
    /// </summary>
    public class DistributedCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<DistributedCacheService> _logger;

        // Lock for preventing cache stampede
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
            // First try to get from cache
            var cached = await GetAsync<T>(key, cancellationToken);
            if (cached != null)
            {
                return cached;
            }

            // Get or create lock for this key to prevent cache stampede
            var lockObj = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

            await lockObj.WaitAsync(cancellationToken);
            try
            {
                // Double-check: another thread may have set the cache while waiting for lock
                cached = await GetAsync<T>(key, cancellationToken);
                if (cached != null)
                {
                    return cached;
                }

                // Call factory method to get data
                var value = await factory(cancellationToken);

                // Cache the result
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

                // Clean up unused lock objects
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
            // First try to get from cache
            var cachedString = await _cache.GetStringAsync(key, cancellationToken);

            if (!string.IsNullOrEmpty(cachedString))
            {
                // Check if it's a null value marker
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

            // Get lock
            var lockObj = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

            await lockObj.WaitAsync(cancellationToken);
            try
            {
                // Double-check
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
                        // Ignore deserialization errors
                    }
                }

                // Call factory method
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
                    // Cache null value to prevent cache penetration
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
            // Note: IDistributedCache does not natively support deletion by prefix
            // If using Redis, this can be implemented via StackExchange.Redis SCAN + DEL
            // For production environments, consider using direct Redis connection
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
