using Dawning.Identity.Application.Interfaces.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Dawning.Identity.Infra.Messaging.Distributed;

/// <summary>
/// 基于 Redis 的分布式锁实现
/// </summary>
public class RedisDistributedLock : IDistributedLock
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<RedisDistributedLock> _logger;
    private readonly string _instanceId;

    public RedisDistributedLock(
        IConnectionMultiplexer? redis,
        ILogger<RedisDistributedLock> logger)
    {
        _redis = redis;
        _logger = logger;
        _instanceId = $"{Environment.MachineName}:{Environment.ProcessId}:{Guid.NewGuid():N}";
    }

    public async Task<IDistributedLockHandle?> TryAcquireAsync(
        string lockKey,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        if (_redis == null || !_redis.IsConnected)
        {
            _logger.LogWarning("Redis not available, returning null lock for resource: {Resource}", lockKey);
            return null;
        }

        var fullLockKey = $"lock:{lockKey}";
        var db = _redis.GetDatabase();

        try
        {
            var acquired = await db.StringSetAsync(
                fullLockKey,
                _instanceId,
                expiry,
                When.NotExists);

            if (acquired)
            {
                _logger.LogDebug("Lock acquired for resource: {Resource}", lockKey);
                return new RedisLockHandle(db, fullLockKey, lockKey, _instanceId, _logger);
            }

            _logger.LogDebug("Failed to acquire lock for resource: {Resource}", lockKey);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acquiring lock for resource: {Resource}", lockKey);
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

        while (DateTime.UtcNow - startTime < timeout)
        {
            var handle = await TryAcquireAsync(lockKey, expiry, cancellationToken);
            if (handle != null)
            {
                return handle;
            }

            await Task.Delay(100, cancellationToken);
        }

        throw new TimeoutException($"Failed to acquire lock for resource '{lockKey}' within {timeout}");
    }

    /// <summary>
    /// Redis 锁句柄实现
    /// </summary>
    private class RedisLockHandle : IDistributedLockHandle
    {
        private readonly IDatabase _db;
        private readonly string _fullLockKey;
        private readonly string _instanceId;
        private readonly ILogger _logger;
        private bool _released;

        public string LockKey { get; }
        public bool IsAcquired => !_released;

        public RedisLockHandle(
            IDatabase db,
            string fullLockKey,
            string lockKey,
            string instanceId,
            ILogger logger)
        {
            _db = db;
            _fullLockKey = fullLockKey;
            LockKey = lockKey;
            _instanceId = instanceId;
            _logger = logger;
        }

        public async Task<bool> ExtendAsync(TimeSpan extension, CancellationToken cancellationToken = default)
        {
            if (_released) return false;

            try
            {
                // 只有持有锁的实例才能延长锁
                var script = @"
                    if redis.call('get', KEYS[1]) == ARGV[1] then
                        return redis.call('pexpire', KEYS[1], ARGV[2])
                    else
                        return 0
                    end
                ";

                var result = await _db.ScriptEvaluateAsync(
                    script,
                    new RedisKey[] { _fullLockKey },
                    new RedisValue[] { _instanceId, (long)extension.TotalMilliseconds });

                var extended = (long)result! == 1;
                if (extended)
                {
                    _logger.LogDebug("Lock extended: {LockKey}", LockKey);
                }
                return extended;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extending lock: {LockKey}", LockKey);
                return false;
            }
        }

        public async Task ReleaseAsync(CancellationToken cancellationToken = default)
        {
            if (_released) return;

            try
            {
                // 只有持有锁的实例才能释放锁
                var script = @"
                    if redis.call('get', KEYS[1]) == ARGV[1] then
                        return redis.call('del', KEYS[1])
                    else
                        return 0
                    end
                ";

                await _db.ScriptEvaluateAsync(
                    script,
                    new RedisKey[] { _fullLockKey },
                    new RedisValue[] { _instanceId });

                _released = true;
                _logger.LogDebug("Lock released: {LockKey}", LockKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing lock: {LockKey}", LockKey);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await ReleaseAsync();
        }
    }
}
