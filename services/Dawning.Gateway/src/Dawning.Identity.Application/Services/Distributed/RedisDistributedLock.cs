using Dawning.Identity.Application.Interfaces.Distributed;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Dawning.Identity.Application.Services.Distributed;

/// <summary>
/// 基于 Redis 的分布式锁实现
/// </summary>
public class RedisDistributedLock : IDistributedLock
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisDistributedLock> _logger;

    public RedisDistributedLock(
        IDistributedCache cache,
        ILogger<RedisDistributedLock> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<IDistributedLockHandle?> TryAcquireAsync(
        string lockKey,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        var lockValue = GenerateLockValue();
        var fullKey = $"lock:{lockKey}";

        try
        {
            // 尝试获取锁（如果键不存在则设置）
            var existing = await _cache.GetStringAsync(fullKey, cancellationToken);

            if (existing != null)
            {
                // 锁已被其他进程持有
                return null;
            }

            // 设置锁
            await _cache.SetStringAsync(
                fullKey,
                lockValue,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiry
                },
                cancellationToken);

            // 验证是否真的获取到锁（防止竞态）
            var verify = await _cache.GetStringAsync(fullKey, cancellationToken);
            if (verify != lockValue)
            {
                return null;
            }

            _logger.LogDebug("Distributed lock acquired: {LockKey}", lockKey);
            return new RedisDistributedLockHandle(_cache, fullKey, lockValue, _logger);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acquiring distributed lock: {LockKey}", lockKey);
            return null;
        }
    }

    public async Task<IDistributedLockHandle> AcquireAsync(
        string lockKey,
        TimeSpan expiry,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        var retryDelay = TimeSpan.FromMilliseconds(50);
        var maxRetryDelay = TimeSpan.FromMilliseconds(500);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var handle = await TryAcquireAsync(lockKey, expiry, cancellationToken);
            if (handle != null)
            {
                return handle;
            }

            var elapsed = DateTime.UtcNow - startTime;
            if (elapsed >= timeout)
            {
                throw new TimeoutException($"Failed to acquire distributed lock '{lockKey}' within {timeout}");
            }

            await Task.Delay(retryDelay, cancellationToken);

            // 指数退避
            retryDelay = TimeSpan.FromMilliseconds(Math.Min(retryDelay.TotalMilliseconds * 2, maxRetryDelay.TotalMilliseconds));
        }
    }

    private static string GenerateLockValue()
    {
        return $"{Environment.MachineName}:{Guid.NewGuid():N}";
    }
}

/// <summary>
/// Redis 分布式锁句柄
/// </summary>
internal class RedisDistributedLockHandle : IDistributedLockHandle
{
    private readonly IDistributedCache _cache;
    private readonly string _fullKey;
    private readonly string _lockValue;
    private readonly ILogger _logger;
    private bool _released;

    public string LockKey => _fullKey.Replace("lock:", "");
    public bool IsAcquired => !_released;

    public RedisDistributedLockHandle(
        IDistributedCache cache,
        string fullKey,
        string lockValue,
        ILogger logger)
    {
        _cache = cache;
        _fullKey = fullKey;
        _lockValue = lockValue;
        _logger = logger;
    }

    public async Task<bool> ExtendAsync(TimeSpan extension, CancellationToken cancellationToken = default)
    {
        if (_released) return false;

        try
        {
            var current = await _cache.GetStringAsync(_fullKey, cancellationToken);
            if (current != _lockValue)
            {
                _released = true;
                return false;
            }

            await _cache.SetStringAsync(
                _fullKey,
                _lockValue,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = extension
                },
                cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extending distributed lock: {LockKey}", LockKey);
            return false;
        }
    }

    public async Task ReleaseAsync(CancellationToken cancellationToken = default)
    {
        if (_released) return;

        try
        {
            var current = await _cache.GetStringAsync(_fullKey, cancellationToken);
            if (current == _lockValue)
            {
                await _cache.RemoveAsync(_fullKey, cancellationToken);
                _logger.LogDebug("Distributed lock released: {LockKey}", LockKey);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing distributed lock: {LockKey}", LockKey);
        }
        finally
        {
            _released = true;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await ReleaseAsync();
    }
}
