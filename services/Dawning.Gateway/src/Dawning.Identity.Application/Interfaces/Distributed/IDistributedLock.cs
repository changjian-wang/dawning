namespace Dawning.Identity.Application.Interfaces.Distributed;

/// <summary>
/// 分布式锁接口
/// </summary>
public interface IDistributedLock
{
    /// <summary>
    /// 尝试获取锁
    /// </summary>
    /// <param name="lockKey">锁键</param>
    /// <param name="expiry">锁过期时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>锁句柄（如果获取成功），null（如果获取失败）</returns>
    Task<IDistributedLockHandle?> TryAcquireAsync(
        string lockKey,
        TimeSpan expiry,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// 获取锁（阻塞直到获取成功或超时）
    /// </summary>
    /// <param name="lockKey">锁键</param>
    /// <param name="expiry">锁过期时间</param>
    /// <param name="timeout">等待超时时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>锁句柄</returns>
    Task<IDistributedLockHandle> AcquireAsync(
        string lockKey,
        TimeSpan expiry,
        TimeSpan timeout,
        CancellationToken cancellationToken = default
    );
}

/// <summary>
/// 分布式锁句柄
/// </summary>
public interface IDistributedLockHandle : IAsyncDisposable
{
    /// <summary>
    /// 锁键
    /// </summary>
    string LockKey { get; }

    /// <summary>
    /// 锁是否仍然有效
    /// </summary>
    bool IsAcquired { get; }

    /// <summary>
    /// 延长锁过期时间
    /// </summary>
    Task<bool> ExtendAsync(TimeSpan extension, CancellationToken cancellationToken = default);

    /// <summary>
    /// 释放锁
    /// </summary>
    Task ReleaseAsync(CancellationToken cancellationToken = default);
}
